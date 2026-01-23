using FluentValidation;

namespace QIMy.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Currency ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters (e.g., EUR, USD)");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required")
            .MaximumLength(100).WithMessage("Currency name must not exceed 100 characters");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Currency symbol is required")
            .MaximumLength(10).WithMessage("Currency symbol must not exceed 10 characters");

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0");
    }
}
