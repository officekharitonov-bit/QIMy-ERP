using FluentValidation;

namespace QIMy.Application.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommandValidator : AbstractValidator<CreateTaxRateCommand>
{
    public CreateTaxRateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(50).WithMessage("Максимум 50 символов");

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100).WithMessage("Ставка должна быть от 0 до 100");
    }
}
