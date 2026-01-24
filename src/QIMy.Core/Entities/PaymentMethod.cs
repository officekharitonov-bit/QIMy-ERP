namespace QIMy.Core.Entities;

public class PaymentMethod : BaseEntity
{
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
