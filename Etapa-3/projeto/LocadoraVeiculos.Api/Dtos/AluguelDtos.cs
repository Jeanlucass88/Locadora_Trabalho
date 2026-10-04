using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Api.Dtos;

public sealed record AluguelResponse(
    int Id,
    int ClienteId,
    string Cliente,
    int VeiculoId,
    string Veiculo,
    string Placa,
    DateTime DataRetirada,
    DateTime DataPrevistaDevolucao,
    DateTime? DataDevolucao,
    int QuilometragemInicial,
    int? QuilometragemFinal,
    decimal ValorDiaria,
    decimal? ValorTotal,
    string Situacao);

public sealed class AluguelCreateRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    public int VeiculoId { get; init; }

    public DateTime DataRetirada { get; init; }
    public DateTime DataPrevistaDevolucao { get; init; }

    [Range(0, int.MaxValue)]
    public int QuilometragemInicial { get; init; }

    public decimal ValorDiaria { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataRetirada == default)
            yield return new ValidationResult("Informe a data de retirada.", [nameof(DataRetirada)]);
        if (DataPrevistaDevolucao <= DataRetirada)
            yield return new ValidationResult("A devolução prevista deve ser posterior à retirada.", [nameof(DataPrevistaDevolucao)]);
        if (ValorDiaria <= 0 || ValorDiaria > 99_999_999.99m)
            yield return new ValidationResult("O valor da diária deve estar entre 0,01 e 99.999.999,99.", [nameof(ValorDiaria)]);
    }
}

public sealed class AluguelUpdateRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    public int VeiculoId { get; init; }

    public DateTime DataRetirada { get; init; }
    public DateTime DataPrevistaDevolucao { get; init; }
    public DateTime? DataDevolucao { get; init; }

    [Range(0, int.MaxValue)]
    public int QuilometragemInicial { get; init; }

    [Range(0, int.MaxValue)]
    public int? QuilometragemFinal { get; init; }

    public decimal ValorDiaria { get; init; }

    public decimal? ValorTotal { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataRetirada == default)
            yield return new ValidationResult("Informe a data de retirada.", [nameof(DataRetirada)]);
        if (DataPrevistaDevolucao <= DataRetirada)
            yield return new ValidationResult("A devolução prevista deve ser posterior à retirada.", [nameof(DataPrevistaDevolucao)]);
        if (DataDevolucao is not null && DataDevolucao < DataRetirada)
            yield return new ValidationResult("A devolução não pode anteceder a retirada.", [nameof(DataDevolucao)]);
        if (QuilometragemFinal is not null && QuilometragemFinal < QuilometragemInicial)
            yield return new ValidationResult("A quilometragem final não pode ser menor que a inicial.", [nameof(QuilometragemFinal)]);
        if (ValorDiaria <= 0 || ValorDiaria > 99_999_999.99m)
            yield return new ValidationResult("O valor da diária deve estar entre 0,01 e 99.999.999,99.", [nameof(ValorDiaria)]);
        if (ValorTotal is < 0 or > 9_999_999_999.99m)
            yield return new ValidationResult("O valor total deve estar entre 0 e 9.999.999.999,99.", [nameof(ValorTotal)]);

        var devolucaoCompleta = DataDevolucao is not null && QuilometragemFinal is not null && ValorTotal is not null;
        var aluguelAberto = DataDevolucao is null && QuilometragemFinal is null && ValorTotal is null;
        if (!devolucaoCompleta && !aluguelAberto)
            yield return new ValidationResult(
                "Data de devolução, quilometragem final e valor total devem ser informados juntos.",
                [nameof(DataDevolucao), nameof(QuilometragemFinal), nameof(ValorTotal)]);
    }
}
