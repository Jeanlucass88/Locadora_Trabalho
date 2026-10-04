namespace LocadoraVeiculos.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cpf { get; set; }
    public required string Email { get; set; }
    public string? Telefone { get; set; }
    public DateOnly DataNascimento { get; set; }
    public required string NumeroCnh { get; set; }
    public DateOnly ValidadeCnh { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Aluguel> Alugueis { get; set; } = [];
}
