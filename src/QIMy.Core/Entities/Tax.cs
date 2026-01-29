using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Tax configuration - combination of TaxRate and Account (Erlöskonto)
/// Defines which tax rate applies to which revenue account
/// </summary>
public class Tax : BaseEntity, IMustHaveBusiness
{
    public int TaxRateId { get; set; }
    public int AccountId { get; set; }
    public int BusinessId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public TaxRate TaxRate { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Business? Business { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
