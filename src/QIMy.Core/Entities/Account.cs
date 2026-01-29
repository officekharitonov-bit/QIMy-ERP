using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Revenue account (Erlöskonto) - e.g., 4000, 4062, 4100, 4112
/// Used to determine tax rates and accounting entries
/// </summary>
public class Account : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public string AccountNumber { get; set; } = string.Empty; // e.g., "4000", "4062"
    public string Name { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty; // Tax code for accounting
    public int? ClientAreaId { get; set; }
    public bool IsForServices { get; set; } = false;
    public int? DefaultTaxRateId { get; set; }
    public string? Comment { get; set; } // Custom invoice comment

    // Navigation properties
    public Business? Business { get; set; }
    public ClientArea? ClientArea { get; set; }
    public TaxRate? DefaultTaxRate { get; set; }
    public ICollection<Tax> Taxes { get; set; } = new List<Tax>();
}
