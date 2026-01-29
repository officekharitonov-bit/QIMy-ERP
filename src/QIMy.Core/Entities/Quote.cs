using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Quote/Offer entity (Angebot - AG)
/// </summary>
public class Quote : BaseEntity, IMustHaveBusiness
{
    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public int ClientId { get; set; }
    public int BusinessId { get; set; }
    public int CurrencyId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public string? Notes { get; set; }
    public string? Terms { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public Business? Business { get; set; }
    public Currency Currency { get; set; } = null!;
    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}

public enum QuoteStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired,
    Cancelled
}
