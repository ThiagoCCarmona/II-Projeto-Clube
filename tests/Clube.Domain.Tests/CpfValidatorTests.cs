using Clube.Shared.Validators;

namespace Clube.Domain.Tests;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void EhValido_DeveAceitarCpfValidoComOuSemMascara(string cpf)
    {
        Assert.True(CpfValidator.EhValido(cpf));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("000.000.000-00")]
    [InlineData("111.111.111-11")]
    [InlineData("22222222222")]
    [InlineData("529.982.247-20")]
    public void EhValido_DeveRejeitarCpfsInvalidosOuComDigitosRepetidos(string? cpf)
    {
        Assert.False(CpfValidator.EhValido(cpf));
    }

    [Fact]
    public void GerarCpfValido_DeveProduzirCpfsValidosEDeterministicos()
    {
        var cpf1 = CpfValidator.GerarCpfValido(1001);
        var cpf2 = CpfValidator.GerarCpfValido(1002);

        Assert.True(CpfValidator.EhValido(cpf1));
        Assert.True(CpfValidator.EhValido(cpf2));
        Assert.NotEqual(cpf1, cpf2);
        Assert.Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", cpf1);
    }

    [Fact]
    public void Formatar_DeveAplicarMascaraQuandoPossuiOnzeDigitos()
    {
        Assert.Equal("529.982.247-25", CpfValidator.Formatar("52998224725"));
        Assert.Equal("52998224725", CpfValidator.Limpar("529.982.247-25"));
    }
}
