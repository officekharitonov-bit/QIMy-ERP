namespace QIMy.Core.Interfaces;

/// <summary>
/// Marker interface for strict tenant isolation.
/// Any entity implementing this MUST belong to exactly one Business.
/// </summary>
public interface IMustHaveBusiness
{
    int BusinessId { get; set; }
}
