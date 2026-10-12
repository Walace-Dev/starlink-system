using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Starlink.Api.Models;

public enum StatusVenda
{
    Ativa,
    Finalizada,
    Cancelada
}

public class Venda
{
    public int Id { get; set; }

    public int DonoId { get; set; }

    [JsonIgnore]
    public Dono? Dono { get; set; }

    [Required]
    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime DataInicioAluguel { get; set; }

    public DateTime? DataFimAluguel { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [MaxLength(300)]
    public string? Observacao { get; set; }

    public StatusVenda Status { get; set; } = StatusVenda.Ativa;
}
