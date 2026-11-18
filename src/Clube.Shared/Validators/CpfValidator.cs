namespace Clube.Shared.Validators;

/// <summary>
/// Utilitário compartilhado de limpeza, formatação e validação algorítmica de CPF (Módulo 11).
/// </summary>
public static class CpfValidator
{
    /// <summary>
    /// Remove pontos, traços e quaisquer caracteres não numéricos.
    /// </summary>
    public static string Limpar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return string.Empty;
        }

        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Formata um CPF de 11 dígitos como 000.000.000-00.
    /// </summary>
    public static string Formatar(string? cpf)
    {
        var limpo = Limpar(cpf);
        if (limpo.Length != 11)
        {
            return cpf?.Trim() ?? string.Empty;
        }

        return $"{limpo[..3]}.{limpo[3..6]}.{limpo[6..9]}-{limpo[9..11]}";
    }

    /// <summary>
    /// Valida sintaticamente e matematicamente o CPF pelos dois dígitos verificadores.
    /// </summary>
    public static bool EhValido(string? cpf)
    {
        var limpo = Limpar(cpf);
        if (limpo.Length != 11)
        {
            return false;
        }

        // Rejeita sequências repetidas conhecidas como inválidas (ex.: 00000000000, 11111111111)
        if (limpo.Distinct().Count() == 1)
        {
            return false;
        }

        Span<int> digitos = stackalloc int[11];
        for (var i = 0; i < 11; i++)
        {
            digitos[i] = limpo[i] - '0';
        }

        var d1 = CalcularDigitoVerificador(digitos[..9]);
        if (digitos[9] != d1)
        {
            return false;
        }

        var d2 = CalcularDigitoVerificador(digitos[..10]);
        return digitos[10] == d2;
    }

    /// <summary>
    /// Gera um CPF válido e formatado de maneira determinística a partir de uma semente numérica.
    /// </summary>
    public static string GerarCpfValido(int semente)
    {
        var valorBase = Math.Abs(semente) % 899_999_999 + 100_000_000;
        var baseTexto = valorBase.ToString("D9");

        // Evita semente que produza todos os 9 dígitos iniciais iguais
        if (baseTexto.Distinct().Count() == 1)
        {
            baseTexto = "123456789";
        }

        Span<int> digitos = stackalloc int[11];
        for (var i = 0; i < 9; i++)
        {
            digitos[i] = baseTexto[i] - '0';
        }

        digitos[9] = CalcularDigitoVerificador(digitos[..9]);
        digitos[10] = CalcularDigitoVerificador(digitos[..10]);

        var bruto = string.Create(11, digitos.ToArray(), static (span, arr) =>
        {
            for (var i = 0; i < 11; i++)
            {
                span[i] = (char)('0' + arr[i]);
            }
        });

        return Formatar(bruto);
    }

    private static int CalcularDigitoVerificador(ReadOnlySpan<int> prefixo)
    {
        var pesoInicial = prefixo.Length + 1;
        var soma = 0;

        for (var i = 0; i < prefixo.Length; i++)
        {
            soma += prefixo[i] * (pesoInicial - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
