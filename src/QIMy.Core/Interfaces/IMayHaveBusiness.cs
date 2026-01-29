namespace QIMy.Core.Interfaces;

/// <summary>
/// Interface for entities that MAY have a Business (nullable BusinessId)
/// Used for global reference data that can be shared across businesses
/// </summary>
public interface IMayHaveBusiness
{
    int? BusinessId { get; set; }
}
