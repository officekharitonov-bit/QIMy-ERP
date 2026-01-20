namespace QIMy.Core.Entities;

public class ExpenseInvoiceItem : BaseEntity
{
    public int ExpenseInvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Category { get; set; }
    
    // Navigation properties
    public ExpenseInvoice ExpenseInvoice { get; set; } = null!;
}
