namespace Starlink.Api.Validation;

public static class CpfCnpjValidator
{
    public static string OnlyDigits(string? value) =>
        value is null ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    public static bool IsValid(string? documento)
    {
        var digitos = OnlyDigits(documento);
        return digitos.Length switch
        {
            11 => IsValidCpf(digitos),
            14 => IsValidCnpj(digitos),
            _ => false
        };
    }

    public static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || TodosDigitosIguais(cpf))
            return false;

        var numeros = cpf.Select(c => c - '0').ToArray();

        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        if (numeros[9] != digito1) return false;

        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return numeros[10] == digito2;
    }

    public static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || TodosDigitosIguais(cnpj))
            return false;

        var numeros = cnpj.Select(c => c - '0').ToArray();

        int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var soma = 0;
        for (var i = 0; i < 12; i++)
            soma += numeros[i] * pesos1[i];
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        if (numeros[12] != digito1) return false;

        int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        soma = 0;
        for (var i = 0; i < 13; i++)
            soma += numeros[i] * pesos2[i];
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return numeros[13] == digito2;
    }

    private static bool TodosDigitosIguais(string valor) => valor.Distinct().Count() == 1;
}
