using QIMy.Core.Interfaces;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Default scoped implementation used by EF Core and services.
/// In Blazor Server, scope = circuit; in ASP.NET Core, scope = request.
/// </summary>
public sealed class CurrentBusinessIdAccessor : ICurrentBusinessIdAccessor
{
    public int? CurrentBusinessId { get; set; }

    public bool BypassTenantFilter { get; set; }
}
