using FluentValidation;

namespace QIMy.Application.TaxRates.Commands.UpdateTaxRate;

public class UpdateTaxRateCommandValidator : AbstractValidator<UpdateTaxRateCommand>
{
    public UpdateTaxRateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID обязателен");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(50).WithMessage("Максимум 50 символов");

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100).WithMessage("Ставка должна быть от 0 до 100");
    }
}
