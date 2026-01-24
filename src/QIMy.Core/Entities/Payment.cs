namespace QIMy.Core.Entities;

public class Payment : BaseEntity
{
    public int? BusinessId { get; set; }
    public int InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public int? PaymentMethodId { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
