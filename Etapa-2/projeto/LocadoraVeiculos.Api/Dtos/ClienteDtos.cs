using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Api.Validation;

namespace LocadoraVeiculos.Api.Dtos;

public sealed record ClienteResponse(
    int Id,
    string Nome,
    string Cpf,
    string Email,
    string? Telefone,
    DateOnly DataNascimento,
    string NumeroCnh,
    DateOnly ValidadeCnh,
    bool Ativo);

public sealed class ClienteRequest : IValidatableObject
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Nome { get; init; } = string.Empty;

    [Required, Cpf]
    public string Cpf { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Phone, StringLength(20)]
    public string? Telefone { get; init; }

    public DateOnly DataNascimento { get; init; }

    [Required, RegularExpression("^[0-9]{11}$", ErrorMessage = "A CNH deve possuir 11 dígitos.")]
    public string NumeroCnh { get; init; } = string.Empty;

    public DateOnly ValidadeCnh { get; init; }
    public bool Ativo { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        if (DataNascimento == default || DataNascimento >= hoje)
            yield return new ValidationResult("A data de nascimento deve estar no passado.", [nameof(DataNascimento)]);
        if (ValidadeCnh == default)
            yield return new ValidationResult("Informe a validade da CNH.", [nameof(ValidadeCnh)]);
    }
}
