namespace QIMy.Core.Entities;

/// <summary>
/// Product/Service entity
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public int? UnitId { get; set; }
    public int? TaxRateId { get; set; }
    public bool IsService { get; set; } = false;
    public int StockQuantity { get; set; } = 0;
    
    // Navigation properties
    public Unit? Unit { get; set; }
    public TaxRate? TaxRate { get; set; }
}
