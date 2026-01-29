using Microsoft.EntityFrameworkCore;
using QIMy.Application.Common.Interfaces;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы со счетами (Accounts) с расширенными методами
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Получить все счета с включенными навигационными свойствами
    /// </summary>
    public async Task<IEnumerable<Account>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.DefaultTaxRate)
            .Include(a => a.ClientArea)
            .Where(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
