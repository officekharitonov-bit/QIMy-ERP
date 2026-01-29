using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

public class Discount : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public Business? Business { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsDefault { get; set; } = false;
}
