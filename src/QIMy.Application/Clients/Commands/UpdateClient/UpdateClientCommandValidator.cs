using FluentValidation;

namespace QIMy.Application.Clients.Commands.UpdateClient;

/// <summary>
/// Валидатор для UpdateClientCommand
/// </summary>
public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0)
            .WithMessage("ID клиента обязателен");

        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Название компании обязательно")
            .MaximumLength(200).WithMessage("Максимальная длина названия - 200 символов");

        RuleFor(c => c.VatNumber)
            .Matches(@"^[A-Z]{2}[A-Z0-9]{2,13}$")
            .When(c => !string.IsNullOrEmpty(c.VatNumber))
            .WithMessage("Неверный формат UID. Пример: ATU12345678");

        RuleFor(c => c.Email)
            .EmailAddress()
            .When(c => !string.IsNullOrEmpty(c.Email))
            .WithMessage("Неверный формат email");

        RuleFor(c => c.ContactPerson)
            .MaximumLength(100)
            .When(c => !string.IsNullOrEmpty(c.ContactPerson))
            .WithMessage("Максимальная длина контактного лица - 100 символов");

        RuleFor(c => c.Phone)
            .MaximumLength(20)
            .When(c => !string.IsNullOrEmpty(c.Phone))
            .WithMessage("Максимальная длина телефона - 20 символов");

        RuleFor(c => c.ClientTypeId)
            .GreaterThan(0)
            .When(c => c.ClientTypeId.HasValue)
            .WithMessage("Неверный идентификатор типа клиента");

        RuleFor(c => c.ClientAreaId)
            .GreaterThan(0)
            .When(c => c.ClientAreaId.HasValue)
            .WithMessage("Неверный идентификатор области клиента");

        RuleFor(c => c.DoubleConfirmed)
            .Equal(true)
            .When(c => c.IgnoreDuplicateWarning)
            .WithMessage("Для сохранения дубликата требуется второе подтверждение (DoubleConfirmed=true)");
    }
}
