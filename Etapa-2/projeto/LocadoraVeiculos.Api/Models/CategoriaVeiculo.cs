namespace LocadoraVeiculos.Api.Models;

public class CategoriaVeiculo
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = [];
}
