using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CpfAttribute : ValidationAttribute
{
    public CpfAttribute() : base("CPF inválido.") { }

    public override bool IsValid(object? value)
    {
        if (value is not string cpf)
            return false;

        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11 || digits.Distinct().Count() == 1)
            return false;

        return CalculateDigit(digits, 9) == digits[9] - '0'
            && CalculateDigit(digits, 10) == digits[10] - '0';
    }

    private static int CalculateDigit(string cpf, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
            sum += (cpf[i] - '0') * (length + 1 - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
