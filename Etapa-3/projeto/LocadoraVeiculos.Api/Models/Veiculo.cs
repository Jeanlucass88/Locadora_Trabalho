namespace LocadoraVeiculos.Api.Models;

public class Veiculo
{
    public int Id { get; set; }
    public int FabricanteId { get; set; }
    public int CategoriaVeiculoId { get; set; }
    public required string Modelo { get; set; }
    public required string Placa { get; set; }
    public required string Chassi { get; set; }
    public int AnoFabricacao { get; set; }
    public int Quilometragem { get; set; }
    public bool Ativo { get; set; } = true;

    public Fabricante Fabricante { get; set; } = null!;
    public CategoriaVeiculo CategoriaVeiculo { get; set; } = null!;
    public ICollection<Aluguel> Alugueis { get; set; } = [];
}
