namespace QIMy.Core.Entities;

public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
