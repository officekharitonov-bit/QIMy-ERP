namespace QIMy.Core.Entities;

/// <summary>
/// Invoice entity (Ausgangsrechnung - AR)
/// </summary>
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public int ClientId { get; set; }
    public int? BusinessId { get; set; }
    public int CurrencyId { get; set; }
    public int? BankAccountId { get; set; }
    public int? PaymentMethodId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public Business? Business { get; set; }
    public Currency Currency { get; set; } = null!;
    public BankAccount? BankAccount { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InvoiceDiscount> InvoiceDiscounts { get; set; } = new List<InvoiceDiscount>();
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Cancelled
}
