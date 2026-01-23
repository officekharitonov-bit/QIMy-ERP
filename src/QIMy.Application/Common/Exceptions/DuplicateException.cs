namespace QIMy.Application.Common.Exceptions;

/// <summary>
/// Исключение, выбрасываемое когда сущность с таким значением уже существует
/// </summary>
public class DuplicateException : Exception
{
    public DuplicateException(string entityName, string field, object value)
        : base($"Сущность \"{entityName}\" с {field} = '{value}' уже существует")
    {
    }
}
