namespace QIMy.Core.Entities;

public class Discount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsDefault { get; set; } = false;
}
