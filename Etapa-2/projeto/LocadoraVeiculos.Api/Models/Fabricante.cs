namespace LocadoraVeiculos.Api.Models;

public class Fabricante
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? PaisOrigem { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = [];
}
