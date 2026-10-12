using System.ComponentModel.DataAnnotations;

namespace Starlink.Api.Validation;

public class CpfCnpjAttribute : ValidationAttribute
{
    public CpfCnpjAttribute() : base("O campo {0} não é um CPF ou CNPJ válido.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string documento || string.IsNullOrWhiteSpace(documento))
            return true; // presença é responsabilidade do [Required]

        return CpfCnpjValidator.IsValid(documento);
    }
}
