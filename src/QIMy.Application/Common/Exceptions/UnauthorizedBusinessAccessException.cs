namespace QIMy.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user tries to access resource from another business
/// </summary>
public class UnauthorizedBusinessAccessException : Exception
{
    public UnauthorizedBusinessAccessException()
        : base("Access denied: Resource belongs to another business.")
    {
    }

    public UnauthorizedBusinessAccessException(string message)
        : base(message)
    {
    }

    public UnauthorizedBusinessAccessException(string entityName, object key, int? expectedBusinessId, int? actualBusinessId)
        : base($"Access denied: {entityName} with Id '{key}' belongs to BusinessId {actualBusinessId}, but current user is operating under BusinessId {expectedBusinessId}.")
    {
    }
}
