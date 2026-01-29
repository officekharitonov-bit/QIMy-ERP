using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Client/Customer entity
/// </summary>
public class Client : BaseEntity, IMustHaveBusiness
{
    /// <summary>
    /// ID бизнеса (мультитенантность)
    /// </summary>
    public int BusinessId { get; set; }

    /// <summary>
    /// Уникальный код клиента (автогенерация на основе ClientArea)
    /// Inland: 200000-229999, EU: 230000-259999, ThirdCountry: 260000-299999
    /// </summary>
    public int? ClientCode { get; set; }

    /// <summary>
    /// Тип клиента (B2B, B2C) - reference to ClientType table
    /// </summary>
    public int? ClientTypeId { get; set; }

    /// <summary>
    /// Географическая область (Inland/EU/ThirdCountry) - reference to ClientArea table
    /// </summary>
    public int? ClientAreaId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }

    // Bank account fields
    public string? IBAN { get; set; }
    public string? BIC { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }

    // Payment terms
    public int PaymentTermsDays { get; set; } = 30;
    public decimal? CreditLimit { get; set; }
    public int? DefaultPaymentMethodId { get; set; }
    public int? CurrencyId { get; set; }

    // Additional fields
    public string? Notes { get; set; }
    public string? CustomField01 { get; set; }
    public string? CustomField02 { get; set; }
    public string? CustomField03 { get; set; }
    public string? CustomField04 { get; set; }
    public string? CustomField05 { get; set; }
    public string? CustomField06 { get; set; }

    // Navigation properties
    public Business? Business { get; set; }
    public ClientType? ClientType { get; set; }
    public ClientArea? ClientArea { get; set; }
    public PaymentMethod? DefaultPaymentMethod { get; set; }
    public Currency? Currency { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
