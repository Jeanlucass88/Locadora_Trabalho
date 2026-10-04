namespace LocadoraVeiculos.Api.Dtos;

public sealed record VeiculoFiltroResponse(
    int Id,
    string Fabricante,
    string Categoria,
    string Modelo,
    string Placa,
    int AnoFabricacao,
    int Quilometragem,
    bool Ativo);

public sealed record AluguelFiltroResponse(
    int Id,
    string Cliente,
    string Cpf,
    string Veiculo,
    string Placa,
    DateTime DataRetirada,
    DateTime DataPrevistaDevolucao,
    DateTime? DataDevolucao,
    decimal ValorDiaria,
    decimal? ValorTotal,
    string Situacao);

public sealed record ClienteSemAluguelResponse(
    int Id,
    string Nome,
    string Cpf,
    string Email,
    bool Ativo);
