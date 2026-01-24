namespace QIMy.Core.Entities;

public class Unit : BaseEntity
{
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
