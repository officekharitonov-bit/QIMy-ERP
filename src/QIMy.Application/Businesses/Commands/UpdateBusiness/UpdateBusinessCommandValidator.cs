using FluentValidation;
using System.Text.RegularExpressions;

namespace QIMy.Application.Businesses.Commands.UpdateBusiness;

public class UpdateBusinessCommandValidator : AbstractValidator<UpdateBusinessCommand>
{
    public UpdateBusinessCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Business ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(200).WithMessage("Business name must not exceed 200 characters.");

        RuleFor(x => x.LegalName)
            .MaximumLength(200).WithMessage("Legal name must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.LegalName));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.VatNumber)
            .MaximumLength(50).WithMessage("VAT number must not exceed 50 characters.")
            .Must(BeValidVatNumber).WithMessage("VAT number must be in format: Country Code (2 letters) + alphanumeric (e.g., ATU12345678)")
            .When(x => !string.IsNullOrEmpty(x.VatNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Website));
    }

    private bool BeValidVatNumber(string? vatNumber)
    {
        if (string.IsNullOrEmpty(vatNumber))
            return true;

        // VAT format: 2 letters (country code) + alphanumeric (2-13 characters)
        return Regex.IsMatch(vatNumber, @"^[A-Z]{2}[A-Z0-9]{2,13}$");
    }
}
