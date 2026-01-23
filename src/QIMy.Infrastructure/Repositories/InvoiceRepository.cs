using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Repositories;

/// <summary>
/// Специализированный репозиторий для Invoice с включением навигационных свойств
/// </summary>
public class InvoiceRepository : Repository<Invoice>
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Currency)
            .Include(i => i.PaymentMethod)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Currency)
            .Include(i => i.PaymentMethod)
            .Include(i => i.BankAccount)
            .Include(i => i.Business)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Items)
                .ThenInclude(item => item.Tax)
                    .ThenInclude(tax => tax != null ? tax.TaxRate : null)
            .Include(i => i.InvoiceDiscounts)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
    }
}
