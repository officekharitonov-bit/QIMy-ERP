namespace QIMy.Core.Entities;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public int? PaymentMethodId { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
