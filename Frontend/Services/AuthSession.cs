namespace StarlinkSystem.Blazor.Services;

public sealed class AuthSession
{
    public bool IsAuthenticated { get; private set; }
    public string Nome { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string Token { get; private set; } = "";
    public int DonoId { get; private set; }
    public bool IsDemo { get; private set; }
    public event Action? Changed;

    public void SignIn(string nome, string email, string token = "", int donoId = 0, bool isDemo = false)
    {
        Nome = nome;
        Email = email;
        Token = token;
        DonoId = donoId;
        IsDemo = isDemo;
        IsAuthenticated = true;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Nome = Email = Token = "";
        DonoId = 0;
        IsDemo = false;
        Changed?.Invoke();
    }
}
