using FluentValidation;

namespace QIMy.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.SKU)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
            .LessThanOrEqualTo(1000000).WithMessage("Price must not exceed 1,000,000");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0")
            .LessThanOrEqualTo(1000000).WithMessage("Stock quantity must not exceed 1,000,000");

        RuleFor(x => x.BusinessId)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("BusinessId is required");

        RuleFor(x => x.DoubleConfirmed)
            .Equal(true)
            .When(x => x.IgnoreDuplicateWarning)
            .WithMessage("Second confirmation (DoubleConfirmed=true) is required to create a duplicate product");
    }
}
