using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedReferenceData(ApplicationDbContext context)
    {
        // Seed ClientAreas
        if (!await context.ClientAreas.AnyAsync())
        {
            var clientAreas = new[]
            {
                new ClientArea { Name = "Inländisch", Code = "1", Description = "Inland - Austria" },
                new ClientArea { Name = "EU", Code = "2", Description = "European Union" },
                new ClientArea { Name = "Ausländisch", Code = "3", Description = "Third Countries (Export)" }
            };
            await context.ClientAreas.AddRangeAsync(clientAreas);
            await context.SaveChangesAsync();
        }

        // Seed ClientTypes
        if (!await context.ClientTypes.AnyAsync())
        {
            var clientTypes = new[]
            {
                new ClientType { Name = "B2B", Code = "1", Description = "Business to Business" },
                new ClientType { Name = "B2C", Code = "2", Description = "Business to Consumer" }
            };
            await context.ClientTypes.AddRangeAsync(clientTypes);
            await context.SaveChangesAsync();
        }

        // Seed TaxRates (Austrian VAT rates)
        if (!await context.TaxRates.AnyAsync())
        {
            var taxRates = new[]
            {
                new TaxRate { Name = "Standard VAT", Rate = 20m, IsDefault = true },
                new TaxRate { Name = "Reduced VAT 10%", Rate = 10m, IsDefault = false },
                new TaxRate { Name = "Reduced VAT 13%", Rate = 13m, IsDefault = false },
                new TaxRate { Name = "VAT Free (Export)", Rate = 0m, IsDefault = false }
            };
            await context.TaxRates.AddRangeAsync(taxRates);
            await context.SaveChangesAsync();
        }

        // Seed Accounts (Revenue accounts / Erlöskonten)
        var domesticArea = await context.ClientAreas.FirstOrDefaultAsync(ca => ca.Code == "1");
        var standardTaxRate = await context.TaxRates.FirstOrDefaultAsync(tr => tr.Rate == 20m);
        var reducedTaxRate10 = await context.TaxRates.FirstOrDefaultAsync(tr => tr.Rate == 10m);
        var reducedTaxRate13 = await context.TaxRates.FirstOrDefaultAsync(tr => tr.Rate == 13m);
        var zeroTaxRate = await context.TaxRates.FirstOrDefaultAsync(tr => tr.Rate == 0m);

        if (!await context.Accounts.AnyAsync() && domesticArea != null)
        {
            var accounts = new[]
            {
                new Account 
                { 
                    AccountNumber = "4000", 
                    Name = "Erlöse 20% USt", 
                    AccountCode = "1", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = standardTaxRate?.Id,
                    IsForServices = false
                },
                new Account 
                { 
                    AccountNumber = "4010", 
                    Name = "Barverkauf 20% USt", 
                    AccountCode = "1", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = standardTaxRate?.Id,
                    IsForServices = false
                },
                new Account 
                { 
                    AccountNumber = "4030", 
                    Name = "Erlöse 20% USt (Inland)", 
                    AccountCode = "1", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = standardTaxRate?.Id,
                    IsForServices = false
                },
                new Account 
                { 
                    AccountNumber = "4062", 
                    Name = "Erlöse 10% USt", 
                    AccountCode = "2", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = reducedTaxRate10?.Id,
                    IsForServices = false
                },
                new Account 
                { 
                    AccountNumber = "4100", 
                    Name = "Erlöse steuerfrei (Export)", 
                    AccountCode = "0", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = zeroTaxRate?.Id,
                    IsForServices = false
                },
                new Account 
                { 
                    AccountNumber = "4112", 
                    Name = "Erlöse 13% USt", 
                    AccountCode = "3", 
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = reducedTaxRate13?.Id,
                    IsForServices = false
                }
            };
            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }

        // Seed Taxes (combinations of TaxRate + Account)
        if (!await context.Taxes.AnyAsync())
        {
            var accounts = await context.Accounts.Include(a => a.DefaultTaxRate).ToListAsync();
            var taxes = new List<Tax>();

            foreach (var account in accounts)
            {
                if (account.DefaultTaxRateId.HasValue)
                {
                    taxes.Add(new Tax
                    {
                        AccountId = account.Id,
                        TaxRateId = account.DefaultTaxRateId.Value,
                        IsActive = true
                    });
                }
            }

            if (taxes.Any())
            {
                await context.Taxes.AddRangeAsync(taxes);
                await context.SaveChangesAsync();
            }
        }

        // Seed Units
        if (!await context.Units.AnyAsync())
        {
            var units = new[]
            {
                new Unit { Name = "Stück", ShortName = "Stk" },
                new Unit { Name = "KG", ShortName = "kg" },
                new Unit { Name = "Meter", ShortName = "m" },
                new Unit { Name = "Liter", ShortName = "l" },
                new Unit { Name = "Karton", ShortName = "Karton" },
                new Unit { Name = "Std", ShortName = "h" },
                new Unit { Name = "Pauschalbetrag", ShortName = "Pauschal" }
            };
            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }
    }
}
