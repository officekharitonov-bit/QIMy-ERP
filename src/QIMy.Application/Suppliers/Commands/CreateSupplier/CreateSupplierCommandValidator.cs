using FluentValidation;

namespace QIMy.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(s => s.BusinessId)
            .GreaterThan(0).WithMessage("BusinessId must be greater than 0.");

        RuleFor(s => s.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.");

        RuleFor(s => s.Email)
            .EmailAddress().When(s => !string.IsNullOrEmpty(s.Email))
            .WithMessage("Invalid email format.");

        RuleFor(s => s.Phone)
            .MaximumLength(50).When(s => !string.IsNullOrEmpty(s.Phone))
            .WithMessage("Phone must not exceed 50 characters.");

        RuleFor(s => s.Address)
            .MaximumLength(500).When(s => !string.IsNullOrEmpty(s.Address))
            .WithMessage("Address must not exceed 500 characters.");

        RuleFor(s => s.City)
            .MaximumLength(100).When(s => !string.IsNullOrEmpty(s.City))
            .WithMessage("City must not exceed 100 characters.");

        RuleFor(s => s.PostalCode)
            .MaximumLength(20).When(s => !string.IsNullOrEmpty(s.PostalCode))
            .WithMessage("Postal code must not exceed 20 characters.");

        RuleFor(s => s.Country)
            .MaximumLength(100).When(s => !string.IsNullOrEmpty(s.Country))
            .WithMessage("Country must not exceed 100 characters.");

        RuleFor(s => s.TaxNumber)
            .MaximumLength(50).When(s => !string.IsNullOrEmpty(s.TaxNumber))
            .WithMessage("Tax number must not exceed 50 characters.");

        RuleFor(s => s.VatNumber)
            .MaximumLength(50).When(s => !string.IsNullOrEmpty(s.VatNumber))
            .WithMessage("VAT number must not exceed 50 characters.");

        RuleFor(s => s.BankAccount)
            .MaximumLength(100).When(s => !string.IsNullOrEmpty(s.BankAccount))
            .WithMessage("Bank account must not exceed 100 characters.");

        // Duplicate confirmation validation
        RuleFor(s => s.DoubleConfirmed)
            .Equal(true)
            .When(s => s.IgnoreDuplicateWarning)
            .WithMessage("DoubleConfirmed must be true when IgnoreDuplicateWarning is true.");
    }
}
