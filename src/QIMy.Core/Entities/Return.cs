using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Return/Credit Note entity (Storno - ST)
/// </summary>
public class Return : BaseEntity, IMustHaveBusiness
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public int ClientId { get; set; }
    public int? OriginalInvoiceId { get; set; }
    public int BusinessId { get; set; }
    public int CurrencyId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public ReturnStatus Status { get; set; } = ReturnStatus.Draft;
    public string? Notes { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public Invoice? OriginalInvoice { get; set; }
    public Business? Business { get; set; }
    public Currency Currency { get; set; } = null!;
    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}

public enum ReturnStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Cancelled,
    Processed
}
