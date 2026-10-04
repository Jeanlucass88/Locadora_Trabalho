namespace LocadoraVeiculos.Api.Models;

public class Aluguel
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public int QuilometragemInicial { get; set; }
    public int? QuilometragemFinal { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal? ValorTotal { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Veiculo Veiculo { get; set; } = null!;
}
