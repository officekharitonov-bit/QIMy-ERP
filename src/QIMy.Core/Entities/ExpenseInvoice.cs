using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Expense Invoice entity (Eingangsrechnung - ER)
/// </summary>
public class ExpenseInvoice : BaseEntity, IMustHaveBusiness
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int SupplierId { get; set; }
    public int BusinessId { get; set; }
    public int CurrencyId { get; set; }
    /// <summary>
    /// Ссылка на Personen Index для подтягивания данных поставщика
    /// </summary>
    public int? PersonenIndexEntryId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;

    public ExpenseInvoiceStatus Status { get; set; } = ExpenseInvoiceStatus.Draft;
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public string? DocumentPath { get; set; }

    // OCR data
    public bool IsOcrProcessed { get; set; } = false;
    public string? OcrData { get; set; }

    // Navigation properties
    public Supplier Supplier { get; set; } = null!;
    public Business? Business { get; set; }
    public Currency Currency { get; set; } = null!;
    public PersonenIndexEntry? PersonenIndexEntry { get; set; }
    public ICollection<ExpenseInvoiceItem> Items { get; set; } = new List<ExpenseInvoiceItem>();
}

public enum ExpenseInvoiceStatus
{
    Draft,
    Approved,
    Paid,
    Rejected
}
