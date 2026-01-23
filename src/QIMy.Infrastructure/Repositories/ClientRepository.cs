using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Repositories;

/// <summary>
/// Специализированный репозиторий для Client с включением навигационных свойств
/// </summary>
public class ClientRepository : Repository<Client>
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.ClientCode)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }
}
