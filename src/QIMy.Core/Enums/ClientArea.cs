namespace QIMy.Core.Enums;

/// <summary>
/// Client area enum - DEPRECATED: Use ClientArea entity instead
/// </summary>
[Obsolete("Use ClientArea entity instead")]
public enum ClientAreaEnum
{
    Inland = 1,        // Австрия (200000-229999)
    EU = 2,            // Европейский Союз (230000-259999)
    ThirdCountry = 3   // Третьи страны (260000-299999)
}
