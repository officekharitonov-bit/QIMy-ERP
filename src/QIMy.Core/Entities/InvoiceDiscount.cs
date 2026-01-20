namespace QIMy.Core.Entities;

/// <summary>
/// Discount applied to invoice
/// </summary>
public class InvoiceDiscount : BaseEntity
{
    public int InvoiceId { get; set; }
    public int DiscountId { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public Discount Discount { get; set; } = null!;
}
