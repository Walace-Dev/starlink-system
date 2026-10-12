using Starlink.Api.Validation;
using Xunit;

namespace Starlink.Api.Tests.Validation;

public class CpfCnpjValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData(null, "")]
    public void OnlyDigits_DeveRemoverFormatacao(
        string? documento,
        string esperado)
    {
        var resultado = CpfCnpjValidator.OnlyDigits(documento);

        Assert.Equal(esperado, resultado);
    }
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("11.222.333/0001-81")]
    public void IsValid_DeveAceitarDocumentosValidos(
        string documento)
    {
        var resultado = CpfCnpjValidator.IsValid(documento);

        Assert.True(resultado);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("12.345.678/0001-00")]
    [InlineData("123")]
    [InlineData("")]
    public void IsValid_DeveRejeitarDocumentosInvalidos(
        string documento)
    {
        var resultado = CpfCnpjValidator.IsValid(documento);
        Assert.False(resultado);
    }
}