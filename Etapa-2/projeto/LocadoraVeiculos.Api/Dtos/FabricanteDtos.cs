using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Api.Dtos;

public sealed record FabricanteResponse(int Id, string Nome, string? PaisOrigem);

public sealed class FabricanteRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(80)]
    public string? PaisOrigem { get; init; }
}
