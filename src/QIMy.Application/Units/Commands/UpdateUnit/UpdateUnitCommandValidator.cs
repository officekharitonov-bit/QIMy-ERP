using FluentValidation;

namespace QIMy.Application.Units.Commands.UpdateUnit;

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Unit ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Unit name is required")
            .MaximumLength(100).WithMessage("Unit name must not exceed 100 characters");

        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Short name is required")
            .MaximumLength(10).WithMessage("Short name must not exceed 10 characters");
    }
}
