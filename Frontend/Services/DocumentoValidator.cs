namespace StarlinkSystem.Blazor.Services;

public static class DocumentoValidator
{
    public static bool IsValid(string? value)
    {
        var d = Digits(value);
        if (d.Length == 11) return Cpf(d);
        if (d.Length == 14) return Cnpj(d);
        return false;
    }
    private static string Digits(string? s) => new((s ?? "").Where(char.IsDigit).ToArray());
    private static bool Repeated(string s) => s.Distinct().Count() == 1;
    private static bool Cpf(string s)
    {
        if (s.Length != 11 || Repeated(s)) return false;
        int sum = 0;
        for (int i = 0; i < 9; i++) sum += (s[i] - '0') * (10 - i);
        int r = sum % 11, d1 = r < 2 ? 0 : 11 - r;
        if (s[9] - '0' != d1) return false;
        sum = 0;
        for (int i = 0; i < 10; i++) sum += (s[i] - '0') * (11 - i);
        r = sum % 11; int d2 = r < 2 ? 0 : 11 - r;
        return s[10] - '0' == d2;
    }
    private static bool Cnpj(string s)
    {
        if (s.Length != 14 || Repeated(s)) return false;
        int[] w1 = [5,4,3,2,9,8,7,6,5,4,3,2];
        int sum = 0; for (int i = 0; i < 12; i++) sum += (s[i] - '0') * w1[i];
        int r = sum % 11, d1 = r < 2 ? 0 : 11 - r;
        if (s[12] - '0' != d1) return false;
        int[] w2 = [6,5,4,3,2,9,8,7,6,5,4,3,2]; sum = 0;
        for (int i = 0; i < 13; i++) sum += (s[i] - '0') * w2[i];
        r = sum % 11; int d2 = r < 2 ? 0 : 11 - r;
        return s[13] - '0' == d2;
    }
}
