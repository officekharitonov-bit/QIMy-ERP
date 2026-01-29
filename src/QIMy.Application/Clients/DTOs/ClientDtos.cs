namespace QIMy.Application.Clients.DTOs;

/// <summary>
/// DTO для отображения клиента
/// </summary>
public record ClientDto
{
    public int Id { get; init; }
    public int? BusinessId { get; init; }
    public int ClientCode { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Website { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TaxNumber { get; init; }

    // Bank account
    public string? IBAN { get; init; }
    public string? BIC { get; init; }
    public string? BankName { get; init; }
    public string? BankAccountNumber { get; init; }

    // Payment terms
    public int PaymentTermsDays { get; init; }
    public decimal? CreditLimit { get; init; }
    public int? DefaultPaymentMethodId { get; init; }
    public string? DefaultPaymentMethodName { get; init; }
    public int? CurrencyId { get; init; }
    public string? CurrencyCode { get; init; }

    // References
    public int? ClientTypeId { get; init; }
    public string? ClientTypeName { get; init; }
    public int? ClientAreaId { get; init; }
    public string? ClientAreaName { get; init; }

    // Additional
    public string? Notes { get; init; }
    public string? CustomField01 { get; init; }
    public string? CustomField02 { get; init; }
    public string? CustomField03 { get; init; }
    public string? CustomField04 { get; init; }
    public string? CustomField05 { get; init; }
    public string? CustomField06 { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO для создания клиента
/// </summary>
public record CreateClientDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Website { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TaxNumber { get; init; }
    public string? IBAN { get; init; }
    public string? BIC { get; init; }
    public string? BankName { get; init; }
    public string? BankAccountNumber { get; init; }
    public int PaymentTermsDays { get; init; } = 30;
    public decimal? CreditLimit { get; init; }
    public int? DefaultPaymentMethodId { get; init; }
    public int? CurrencyId { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
    public string? Notes { get; init; }
    public string? CustomField01 { get; init; }
    public string? CustomField02 { get; init; }
    public string? CustomField03 { get; init; }
    public string? CustomField04 { get; init; }
    public string? CustomField05 { get; init; }
    public string? CustomField06 { get; init; }
}

/// <summary>
/// DTO для обновления клиента
/// </summary>
public record UpdateClientDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Website { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TaxNumber { get; init; }
    public string? IBAN { get; init; }
    public string? BIC { get; init; }
    public string? BankName { get; init; }
    public string? BankAccountNumber { get; init; }
    public int PaymentTermsDays { get; init; } = 30;
    public decimal? CreditLimit { get; init; }
    public int? DefaultPaymentMethodId { get; init; }
    public int? CurrencyId { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
    public string? Notes { get; init; }
    public string? CustomField01 { get; init; }
    public string? CustomField02 { get; init; }
    public string? CustomField03 { get; init; }
    public string? CustomField04 { get; init; }
    public string? CustomField05 { get; init; }
    public string? CustomField06 { get; init; }
}
