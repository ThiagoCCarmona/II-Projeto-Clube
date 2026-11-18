using Clube.Application.Payments;

namespace Clube.Application.Tests;

public class PixPayloadGeneratorTests
{
    [Fact]
    public void GerarPayload_DeveGerarCodigoEmvValidoComCrc16Ccitt()
    {
        var payload = PixPayloadGenerator.GerarPayload(
            chavePix: "12345678000199",
            nomeRecebedor: "Rotary Foz Ponte",
            cidadeRecebedor: "Foz do Iguacu",
            valor: 150.00m,
            txId: "MENSAL202611");

        Assert.StartsWith("000201", payload);
        Assert.Contains("BR.GOV.BCB.PIX", payload);
        Assert.Contains("12345678000199", payload);
        Assert.Contains("5303986", payload);
        Assert.Contains("5406150.00", payload);
        Assert.Contains("5802BR", payload);
        Assert.Contains("6304", payload);
        Assert.True(PixPayloadGenerator.ValidarChecksum(payload));
    }

    [Fact]
    public void GerarPayload_DeveNormalizarAcentosETamanhosMaximos()
    {
        var payload = PixPayloadGenerator.GerarPayload(
            chavePix: "pix@iateclube.local",
            nomeRecebedor: "Associação Náutica São João da Barra Grande",
            cidadeRecebedor: "Foz do Iguaçu do Paraná",
            valor: 200.50m);

        Assert.DoesNotContain("ç", payload);
        Assert.DoesNotContain("ã", payload);
        Assert.DoesNotContain("á", payload);
        Assert.True(PixPayloadGenerator.ValidarChecksum(payload));
    }

    [Fact]
    public void ValidarChecksum_DeveRetornarFalse_QuandoPayloadAlterado()
    {
        var payload = PixPayloadGenerator.GerarPayload("pix@clube.local", "Clube", "Foz", 100m);
        var adulterado = payload.Replace("100.00", "999.00");

        Assert.False(PixPayloadGenerator.ValidarChecksum(adulterado));
    }
}
