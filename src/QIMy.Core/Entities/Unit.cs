namespace QIMy.Core.Entities;

public class Unit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
