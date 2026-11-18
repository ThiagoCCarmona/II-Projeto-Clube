using System.Globalization;
using System.Text;

namespace Clube.Application.Payments;

/// <summary>
/// Gerador e validador de código PIX Copia-e-Cola no padrão BACEN EMV QRCPS-MPM com CRC16-CCITT.
/// </summary>
public static class PixPayloadGenerator
{
    /// <summary>
    /// Gera uma string EMV Copia-e-Cola válida contendo chave PIX, recebedor, cidade, valor e CRC16.
    /// </summary>
    public static string GerarPayload(
        string chavePix,
        string nomeRecebedor,
        string cidadeRecebedor,
        decimal valor,
        string txId = "CLUBE01")
    {
        if (string.IsNullOrWhiteSpace(chavePix))
        {
            throw new ArgumentException("A chave PIX do clube é obrigatória.", nameof(chavePix));
        }

        var nomeLimpo = NormalizarTextoEmv(nomeRecebedor, 25, "CLUBE");
        var cidadeLimpa = NormalizarTextoEmv(cidadeRecebedor, 15, "BRASIL");
        var txIdLimpo = NormalizarTxId(txId);

        var gui = FormatarCampo("00", "BR.GOV.BCB.PIX");
        var chave = FormatarCampo("01", chavePix.Trim());
        var merchantAccount = FormatarCampo("26", gui + chave);

        var sb = new StringBuilder();
        sb.Append(FormatarCampo("00", "01"));
        sb.Append(merchantAccount);
        sb.Append(FormatarCampo("52", "0000"));
        sb.Append(FormatarCampo("53", "986"));

        if (valor > 0)
        {
            sb.Append(FormatarCampo("54", valor.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        sb.Append(FormatarCampo("58", "BR"));
        sb.Append(FormatarCampo("59", nomeLimpo));
        sb.Append(FormatarCampo("60", cidadeLimpa));
        sb.Append(FormatarCampo("62", FormatarCampo("05", txIdLimpo)));
        sb.Append("6304");

        var payloadSemCrc = sb.ToString();
        var crc = CalcularCrc16(payloadSemCrc);
        return payloadSemCrc + crc;
    }

    /// <summary>
    /// Calcula o checksum CRC16-CCITT (polinômio 0x1021, valor inicial 0xFFFF).
    /// </summary>
    public static string CalcularCrc16(string payloadSemCrc)
    {
        const ushort polinomio = 0x1021;
        ushort resultado = 0xFFFF;

        var bytes = Encoding.UTF8.GetBytes(payloadSemCrc);
        foreach (var b in bytes)
        {
            resultado ^= (ushort)(b << 8);
            for (var bit = 0; bit < 8; bit++)
            {
                if ((resultado & 0x8000) != 0)
                {
                    resultado = (ushort)((resultado << 1) ^ polinomio);
                }
                else
                {
                    resultado <<= 1;
                }
            }
        }

        return resultado.ToString("X4", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Valida se um payload PIX EMV possui sufixo 6304XXXX com CRC16-CCITT válido.
    /// </summary>
    public static bool ValidarChecksum(string? payloadCompleto)
    {
        if (string.IsNullOrWhiteSpace(payloadCompleto) || payloadCompleto.Length < 12)
        {
            return false;
        }

        var indiceCrc = payloadCompleto.Length - 8;
        if (!payloadCompleto.AsSpan(indiceCrc, 4).SequenceEqual("6304"))
        {
            return false;
        }

        var corpo = payloadCompleto[..^4];
        var crcInformado = payloadCompleto[^4..];
        var crcCalculado = CalcularCrc16(corpo);

        return string.Equals(crcInformado, crcCalculado, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatarCampo(string id, string valor) =>
        $"{id}{valor.Length:D2}{valor}";

    private static string NormalizarTextoEmv(string? texto, int tamanhoMaximo, string fallback)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return fallback;
        }

        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var ch in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark &&
                ch >= 32 && ch <= 126)
            {
                sb.Append(ch);
            }
        }

        var limpo = sb.ToString().Trim();
        if (string.IsNullOrEmpty(limpo))
        {
            return fallback;
        }

        return limpo.Length <= tamanhoMaximo ? limpo : limpo[..tamanhoMaximo];
    }

    private static string NormalizarTxId(string? txId)
    {
        if (string.IsNullOrWhiteSpace(txId))
        {
            return "CLUBE01";
        }

        var alfanum = new string(txId.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(alfanum))
        {
            return "CLUBE01";
        }

        return alfanum.Length <= 25 ? alfanum : alfanum[..25];
    }
}
