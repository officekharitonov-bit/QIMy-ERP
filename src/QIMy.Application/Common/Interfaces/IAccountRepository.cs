using QIMy.Core.Entities;

namespace QIMy.Application.Common.Interfaces;

/// <summary>
/// Репозиторий для работы со счетами (Accounts) с дополнительными методами
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Получить все счета с включенными навигационными свойствами (DefaultTaxRate, ClientArea)
    /// </summary>
    Task<IEnumerable<Account>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default);
}
