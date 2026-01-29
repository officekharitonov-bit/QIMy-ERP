using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Bank account information for business
/// </summary>
public class BankAccount : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string BIC { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? BLZ { get; set; } // Bankleitzahl (German bank code)
    public int? DefaultCurrencyId { get; set; }
    public bool IsDefault { get; set; } = false;

    // Navigation properties
    public Business Business { get; set; } = null!;
    public Currency? DefaultCurrency { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
