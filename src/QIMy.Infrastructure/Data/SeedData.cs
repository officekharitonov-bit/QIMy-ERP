using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedReferenceData(ApplicationDbContext context)
    {
        // Get or create first business for seeding
        var business = await context.Businesses.FirstOrDefaultAsync();
        if (business == null)
        {
            business = new Business
            {
                Name = "Default Company",
                LegalName = "Default Company Ltd",
                Email = "info@defaultcompany.com"
            };
            await context.Businesses.AddAsync(business);
            await context.SaveChangesAsync();
        }
        
        var businessId = business.Id;

        // Seed ClientAreas
        if (!await context.ClientAreas.AnyAsync())
        {
            var clientAreas = new[]
            {
                new ClientArea { Name = "Inländisch", Code = "1", Description = "Inland - Austria", BusinessId = businessId },
                new ClientArea { Name = "EU", Code = "2", Description = "European Union", BusinessId = businessId },
                new ClientArea { Name = "Ausländisch", Code = "3", Description = "Third Countries (Export)", BusinessId = businessId }
            };
            await context.ClientAreas.AddRangeAsync(clientAreas);
            await context.SaveChangesAsync();
        }

        // Seed ClientTypes
        if (!await context.ClientTypes.AnyAsync())
        {
            var clientTypes = new[]
            {
                new ClientType { Name = "B2B", Code = "1", Description = "Business to Business", BusinessId = businessId },
                new ClientType { Name = "B2C", Code = "2", Description = "Business to Consumer", BusinessId = businessId }
            };
            await context.ClientTypes.AddRangeAsync(clientTypes);
            await context.SaveChangesAsync();
        }

        // Seed TaxRates - Will be populated by VatRateUpdateService from Vatlayer API
        // Seed only Austria rates initially (for backward compatibility)
        if (!await context.TaxRates.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var taxRates = new[]
            {
                new TaxRate 
                { 
                    CountryCode = "AT",
                    CountryName = "Austria",
                    Name = "Standard VAT (AT)", 
                    Rate = 20m, 
                    RateType = TaxRateType.Standard,
                    IsDefault = true, 
                    BusinessId = businessId,
                    EffectiveFrom = now,
                    EffectiveUntil = null,
                    Source = "Manual",
                    Notes = "Initial seed - will be updated by Vatlayer API"
                },
                new TaxRate 
                { 
                    CountryCode = "AT",
                    CountryName = "Austria",
                    Name = "Reduced VAT 10% (AT)", 
                    Rate = 10m, 
                    RateType = TaxRateType.Reduced,
                    IsDefault = false, 
                    BusinessId = businessId,
                    EffectiveFrom = now,
                    EffectiveUntil = null,
                    Source = "Manual"
                },
                new TaxRate 
                { 
                    CountryCode = "AT",
                    CountryName = "Austria",
                    Name = "Reduced VAT 13% (AT)", 
                    Rate = 13m, 
                    RateType = TaxRateType.SuperReduced,
                    IsDefault = false, 
                    BusinessId = businessId,
                    EffectiveFrom = now,
                    EffectiveUntil = null,
                    Source = "Manual"
                },
                new TaxRate 
                { 
                    CountryCode = "AT",
                    CountryName = "Austria",
                    Name = "VAT Free Export (AT)", 
                    Rate = 0m, 
                    RateType = TaxRateType.Zero,
                    IsDefault = false, 
                    BusinessId = businessId,
                    EffectiveFrom = now,
                    EffectiveUntil = null,
                    Source = "Manual"
                }
            };
            await context.TaxRates.AddRangeAsync(taxRates);
            await context.SaveChangesAsync();
            
            // Note: VatRateUpdateService will populate EU countries automatically on first run
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
                    IsForServices = false,
                    BusinessId = businessId
                },
                new Account
                {
                    AccountNumber = "4010",
                    Name = "Barverkauf 20% USt",
                    AccountCode = "1",
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = standardTaxRate?.Id,
                    IsForServices = false,
                    BusinessId = businessId
                },
                new Account
                {
                    AccountNumber = "4030",
                    Name = "Erlöse 20% USt (Inland)",
                    AccountCode = "1",
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = standardTaxRate?.Id,
                    IsForServices = false,
                    BusinessId = businessId
                },
                new Account
                {
                    AccountNumber = "4062",
                    Name = "Erlöse 10% USt",
                    AccountCode = "2",
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = reducedTaxRate10?.Id,
                    IsForServices = false,
                    BusinessId = businessId
                },
                new Account
                {
                    AccountNumber = "4100",
                    Name = "Erlöse steuerfrei (Export)",
                    AccountCode = "0",
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = zeroTaxRate?.Id,
                    IsForServices = false,
                    BusinessId = businessId
                },
                new Account
                {
                    AccountNumber = "4112",
                    Name = "Erlöse 13% USt",
                    AccountCode = "3",
                    ClientAreaId = domesticArea.Id,
                    DefaultTaxRateId = reducedTaxRate13?.Id,
                    IsForServices = false,
                    BusinessId = businessId
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
                        IsActive = true,
                        BusinessId = businessId
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
                new Unit { Name = "Stück", ShortName = "Stk", BusinessId = businessId },
                new Unit { Name = "KG", ShortName = "kg", BusinessId = businessId },
                new Unit { Name = "Meter", ShortName = "m", BusinessId = businessId },
                new Unit { Name = "Liter", ShortName = "l", BusinessId = businessId },
                new Unit { Name = "Karton", ShortName = "Karton", BusinessId = businessId },
                new Unit { Name = "Std", ShortName = "h", BusinessId = businessId },
                new Unit { Name = "Pauschalbetrag", ShortName = "Pauschal", BusinessId = businessId }
            };
            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }

        // Seed Currencies
        if (!await context.Currencies.AnyAsync())
        {
            var currencies = new[]
            {
                new Currency
                {
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    ExchangeRate = 1.0m,
                    IsDefault = true,
                    BusinessId = businessId
                },
                new Currency
                {
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    ExchangeRate = 1.1m,
                    IsDefault = false,
                    BusinessId = businessId
                },
                new Currency
                {
                    Code = "RUB",
                    Name = "Russian Ruble",
                    Symbol = "₽",
                    ExchangeRate = 0.01m,
                    IsDefault = false,
                    BusinessId = businessId
                },
                new Currency
                {
                    Code = "CHF",
                    Name = "Swiss Franc",
                    Symbol = "CHF",
                    ExchangeRate = 1.05m,
                    IsDefault = false,
                    BusinessId = businessId
                },
                new Currency
                {
                    Code = "GBP",
                    Name = "British Pound",
                    Symbol = "£",
                    ExchangeRate = 1.17m,
                    IsDefault = false,
                    BusinessId = businessId
                }
            };
            await context.Currencies.AddRangeAsync(currencies);
            await context.SaveChangesAsync();
        }

        // Seed PaymentMethods
        if (!await context.PaymentMethods.AnyAsync())
        {
            var paymentMethods = new[]
            {
                new PaymentMethod { Name = "Überweisung", IsDefault = true, BusinessId = businessId },
                new PaymentMethod { Name = "Barzahlung", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Kreditkarte", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "PayPal", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Lastschrift", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Scheck", IsDefault = false, BusinessId = businessId }
            };
            await context.PaymentMethods.AddRangeAsync(paymentMethods);
            await context.SaveChangesAsync();
        }
    }
}
