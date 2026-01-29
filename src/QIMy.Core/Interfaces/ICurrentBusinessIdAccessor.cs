namespace QIMy.Core.Interfaces;

/// <summary>
/// Provides access to the currently selected BusinessId for the active scope (request/circuit).
/// This is used by EF Core global query filters and SaveChanges guardrails to enforce strict tenant isolation.
/// </summary>
public interface ICurrentBusinessIdAccessor
{
    /// <summary>
    /// Currently selected BusinessId for the active scope.
    /// Null means "no business selected".
    /// </summary>
    int? CurrentBusinessId { get; set; }

    /// <summary>
    /// Emergency escape hatch for infrastructure tasks (migrations/seed/admin maintenance).
    /// When true, tenant query filters and guards may be bypassed.
    /// Keep false for normal application execution.
    /// </summary>
    bool BypassTenantFilter { get; set; }
}
