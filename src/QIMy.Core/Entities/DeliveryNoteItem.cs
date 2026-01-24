namespace QIMy.Core.Entities;

public class DeliveryNoteItem : BaseEntity
{
    public int DeliveryNoteId { get; set; }
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal QuantityDelivered { get; set; } = 0;

    // Navigation properties
    public DeliveryNote DeliveryNote { get; set; } = null!;
    public Product? Product { get; set; }
}
