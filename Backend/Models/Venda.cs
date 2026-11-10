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

public class Venda : IValidatableObject
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
    // Validação de negócio aplicada pelo ASP.NET Core aos DTOs recebidos.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Valor <= 0)
            yield return new ValidationResult(
                "O valor precisa ser maior que zero.", new[] { nameof(Valor) });

        if (DataInicioAluguel == default)
            yield return new ValidationResult(
                "Informe a data inicial.", new[] { nameof(DataInicioAluguel) });

        if (DataFimAluguel is DateTime fim && fim < DataInicioAluguel)
            yield return new ValidationResult(
                "A data final não pode ser anterior à data inicial.",
                new[] { nameof(DataFimAluguel) });
    }

}
