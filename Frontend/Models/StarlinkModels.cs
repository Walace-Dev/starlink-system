using System.ComponentModel.DataAnnotations;

namespace StarlinkSystem.Blazor.Models;

public enum StatusVenda { Ativa = 0, Finalizada = 1, Cancelada = 2 }

public sealed class ClienteDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Informe CPF ou CNPJ.")]
    [StringLength(20)]
    public string Documento { get; set; } = "";
    [StringLength(20)] public string? Telefone { get; set; }
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)] public string? Email { get; set; }
    [StringLength(250)] public string? Endereco { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}

public sealed class VendaDto
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente.")]
    public int ClienteId { get; set; }
    public ClienteDto? Cliente { get; set; }
    [Required] public DateTime DataInicioAluguel { get; set; } = DateTime.Today;
    public DateTime? DataFimAluguel { get; set; }
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; }
    [StringLength(300)] public string? Observacao { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.Ativa;
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
}

public sealed class LoginRequest
{
    [Required, EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = "";
    [Required] public string Senha { get; set; } = "";
}

public sealed class RegistroRequest
{
    [Required, StringLength(150)] public string Nome { get; set; } = "";
    [Required, EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = "";
    [Required, MinLength(6, ErrorMessage = "Use pelo menos 6 caracteres.")]
    public string Senha { get; set; } = "";
}

public sealed class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiraEm { get; set; }
    public int DonoId { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
}
