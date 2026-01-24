namespace QIMy.Core.Entities;

/// <summary>
/// Audit log for VAT rate changes - tracks all historical changes
/// </summary>
public class VatRateChangeLog : BaseEntity
{
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2): AT, DE, GB, etc.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Country name: Austria, Germany, etc.
    /// </summary>
    public string CountryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of rate that changed
    /// </summary>
    public TaxRateType RateType { get; set; } = TaxRateType.Standard;
    
    /// <summary>
    /// Previous rate (null if new rate added)
    /// </summary>
    public decimal? OldRate { get; set; }
    
    /// <summary>
    /// New rate
    /// </summary>
    public decimal NewRate { get; set; }
    
    /// <summary>
    /// Date and time when the change was detected/applied
    /// </summary>
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Reason for change (e.g., "EU Directive 2026/XXX")
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Source of change: VatlayerAPI, Manual, Excel
    /// </summary>
    public string Source { get; set; } = "VatlayerAPI";
    
    /// <summary>
    /// Whether admin was notified about this change
    /// </summary>
    public bool IsNotified { get; set; } = false;
    
    /// <summary>
    /// When admin was notified
    /// </summary>
    public DateTime? NotifiedAt { get; set; }
    
    /// <summary>
    /// User who made the change (for manual changes)
    /// </summary>
    public string? ChangedBy { get; set; }
}
