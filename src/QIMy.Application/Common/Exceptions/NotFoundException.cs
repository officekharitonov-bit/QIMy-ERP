namespace QIMy.Application.Common.Exceptions;

/// <summary>
/// Исключение, выбрасываемое когда сущность не найдена
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Сущность \"{entityName}\" с ключом ({key}) не найдена")
    {
    }
}
