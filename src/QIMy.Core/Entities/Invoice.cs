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
    /// <summary>
    /// Ссылка на Personen Index для подтягивания данных клиента и налогов
    /// </summary>
    public int? PersonenIndexEntryId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }
    public string? Terms { get; set;}

    // Austrian invoice type (Rechnungsmerkmale)
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Domestic;
    
    // Tax-specific fields for different invoice types
    /// <summary>
    /// For Reverse Charge invoices - indicates VAT liability is with customer
    /// </summary>
    public bool IsReverseCharge { get; set; } = false;
    
    /// <summary>
    /// For Kleinunternehmer (small business) - no VAT charged
    /// </summary>
    public bool IsSmallBusinessExemption { get; set; } = false;
    
    /// <summary>
    /// For Exportrechnung - tax-free export
    /// </summary>
    public bool IsTaxFreeExport { get; set; } = false;
    
    /// <summary>
    /// For Innergemeinschaftliche Lieferung (intra-EU supply)
    /// </summary>
    public bool IsIntraEUSale { get; set; } = false;

    // BMD NTCS FIBU posting parameters
    /// <summary>
    /// Austrian tax code (Steuercode 1-99) for FIBU posting
    /// </summary>
    public int? Steuercode { get; set; }
    
    /// <summary>
    /// Revenue account number (Erlöskonto) for FIBU posting
    /// Example: 4000 (standard), 4062 (Kleinunternehmer)
    /// </summary>
    public string? Konto { get; set; }
    
    /// <summary>
    /// Tax percentage (Prozentsatz) - e.g., 20.0 for 20% VAT
    /// </summary>
    public decimal? Proz { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public Business? Business { get; set; }
    public Currency Currency { get; set; } = null!;
    public BankAccount? BankAccount { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PersonenIndexEntry? PersonenIndexEntry { get; set; }
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

/// <summary>
/// Austrian invoice types based on Rechnungsmerkmale requirements
/// </summary>
public enum InvoiceType
{
    /// <summary>Inland - domestic supply within Austria with standard 20% VAT</summary>
    Domestic = 0,
    
    /// <summary>Exportrechnung - tax-free export, 0% VAT</summary>
    Export = 1,
    
    /// <summary>Innergemeinschaftliche Lieferung - intra-EU supply, 0% VAT in AT, customer reports VAT</summary>
    IntraEUSale = 2,
    
    /// <summary>Reverse Charge - VAT liability on customer, 0% VAT in invoice</summary>
    ReverseCharge = 3,
    
    /// <summary>Kleinunternehmer - small business exemption, no VAT</summary>
    SmallBusinessExemption = 4,
    
    /// <summary>Dreiecksgeschäfte - triangular transactions</summary>
    TriangularTransaction = 5
}
