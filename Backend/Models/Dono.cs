using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Starlink.Api.Models;

public class Dono
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    [JsonIgnore]
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
