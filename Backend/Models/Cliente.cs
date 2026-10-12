using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Starlink.Api.Validation;

namespace Starlink.Api.Models;

public class Cliente
{
    public int Id { get; set; }

    public int DonoId { get; set; }

    [JsonIgnore]
    public Dono? Dono { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(20), CpfCnpj]
    public string Documento { get; set; } = string.Empty; // CPF ou CNPJ

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Endereco { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
