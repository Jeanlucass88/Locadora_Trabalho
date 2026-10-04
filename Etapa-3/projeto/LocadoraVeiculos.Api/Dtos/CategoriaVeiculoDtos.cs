using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Api.Dtos;

public sealed record CategoriaVeiculoResponse(int Id, string Nome, string? Descricao);

public sealed class CategoriaVeiculoRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(250)]
    public string? Descricao { get; init; }
}
