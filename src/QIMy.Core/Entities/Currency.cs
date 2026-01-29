using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

public class Currency : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public Business? Business { get; set; }
    public string Code { get; set; } = string.Empty; // EUR, USD, etc.
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public bool IsDefault { get; set; } = false;
}
