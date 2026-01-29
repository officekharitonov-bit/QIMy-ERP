using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Client type classification (B2B, B2C)
/// </summary>
public class ClientType : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // 1=B2B, 2=B2C
    public string? Description { get; set; }

    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}
