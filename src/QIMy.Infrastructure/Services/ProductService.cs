using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.TaxRate)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.TaxRate)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.IsDeleted = false;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Product with ID {product.Id} not found");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.SKU = product.SKU;
        existing.Price = product.Price;
        existing.UnitId = product.UnitId;
        existing.TaxRateId = product.TaxRateId;
        existing.IsService = product.IsService;
        existing.StockQuantity = product.StockQuantity;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.TaxRate)
            .Where(p => !p.IsDeleted && 
                   (p.Name.Contains(searchTerm) || 
                    p.Description!.Contains(searchTerm) || 
                    p.SKU!.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByIsServiceAsync(bool isService)
    {
        return await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.TaxRate)
            .Where(p => !p.IsDeleted && p.IsService == isService)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
