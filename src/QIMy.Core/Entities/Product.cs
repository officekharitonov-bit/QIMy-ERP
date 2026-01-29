using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Product/Service entity
/// </summary>
public class Product : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? PartNumber { get; set; }
    public string? Brand { get; set; }
    public string? AdditionalName { get; set; }
    public decimal Price { get; set; }
    public int? UnitId { get; set; }
    public int? TaxRateId { get; set; }
    public bool IsService { get; set; } = false;
    public int StockQuantity { get; set; } = 0;
    public string? TareUnit { get; set; }
    public decimal? UnitsInTare { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Business? Business { get; set; }
    public Unit? Unit { get; set; }
    public TaxRate? TaxRate { get; set; }
}
