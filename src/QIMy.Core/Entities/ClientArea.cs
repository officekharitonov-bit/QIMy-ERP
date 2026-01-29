using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Client area classification (Inland, EU, Export)
/// </summary>
public class ClientArea : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // 1=Inland, 2=EU, 3=Export
    public string? Description { get; set; }

    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
