using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Delivery Note entity (Lieferschein - LS)
/// Non-financial document, just tracks shipments
/// </summary>
public class DeliveryNote : BaseEntity, IMustHaveBusiness
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    public int ClientId { get; set; }
    public int? InvoiceId { get; set; }
    public int BusinessId { get; set; }

    public DeliveryNoteStatus Status { get; set; } = DeliveryNoteStatus.Draft;
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public Business? Business { get; set; }
    public ICollection<DeliveryNoteItem> Items { get; set; } = new List<DeliveryNoteItem>();
}

public enum DeliveryNoteStatus
{
    Draft,
    Pending,
    Shipped,
    Delivered,
    Cancelled
}
