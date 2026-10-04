using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Api.Dtos;

public sealed record VeiculoResponse(
    int Id,
    int FabricanteId,
    string Fabricante,
    int CategoriaVeiculoId,
    string Categoria,
    string Modelo,
    string Placa,
    string Chassi,
    int AnoFabricacao,
    int Quilometragem,
    bool Ativo);

public sealed class VeiculoRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int FabricanteId { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoriaVeiculoId { get; init; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Modelo { get; init; } = string.Empty;

    [Required, RegularExpression("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$",
        ErrorMessage = "A placa deve estar no formato ABC1234 ou ABC1D23.")]
    public string Placa { get; init; } = string.Empty;

    [Required, RegularExpression("^[A-HJ-NPR-Z0-9]{17}$",
        ErrorMessage = "O chassi deve possuir 17 caracteres válidos.")]
    public string Chassi { get; init; } = string.Empty;

    [Range(1886, 2100)]
    public int AnoFabricacao { get; init; }

    [Range(0, int.MaxValue)]
    public int Quilometragem { get; init; }

    public bool Ativo { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnoFabricacao > DateTime.UtcNow.Year + 1)
            yield return new ValidationResult("O ano de fabricação não pode ser futuro.", [nameof(AnoFabricacao)]);
    }
}
