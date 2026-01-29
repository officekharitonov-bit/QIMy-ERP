using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedReferenceData(ApplicationDbContext context)
    {
        // Get or create first business for seeding
        var business = await context.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (business == null)
        {
            business = new Business
            {
                Name = "Шаблон",
                LegalName = "Шаблон (Template)",
                Email = "template@qimy.com"
            };
            await context.Businesses.AddAsync(business);
            await context.SaveChangesAsync();
        }

        var businessId = business.Id;

        // Seed ClientAreas
        if (!await context.ClientAreas.IgnoreQueryFilters().AnyAsync())
        {
            var clientAreas = new[]
            {
                new ClientArea { Name = "Inländisch", Code = "1", Description = "Inland - Austria", BusinessId = businessId },
                new ClientArea { Name = "EU", Code = "2", Description = "European Union", BusinessId = businessId },
                new ClientArea { Name = "Ausländisch", Code = "3", Description = "Third Countries (Export)", BusinessId = businessId },
                new ClientArea { Name = "Deutschland", Code = "DE", Description = "Germany", BusinessId = businessId },
                new ClientArea { Name = "Schweiz", Code = "CH", Description = "Switzerland", BusinessId = businessId },
                new ClientArea { Name = "Großbritannien", Code = "GB", Description = "Great Britain", BusinessId = businessId },
                new ClientArea { Name = "Benelux", Code = "BENELUX", Description = "Belgium, Netherlands, Luxembourg", BusinessId = businessId },
                new ClientArea { Name = "Nordische Länder", Code = "NORDIC", Description = "Scandinavia region", BusinessId = businessId },
                new ClientArea { Name = "Osteuropa", Code = "EASTEU", Description = "Poland, Czech Republic, Hungary", BusinessId = businessId }
            };
            await context.ClientAreas.AddRangeAsync(clientAreas);
            await context.SaveChangesAsync();
        }

        // Seed ClientTypes
        if (!await context.ClientTypes.IgnoreQueryFilters().AnyAsync())
        {
            var clientTypes = new[]
            {
                new ClientType { Name = "B2B", Code = "1", Description = "Business to Business", BusinessId = businessId },
                new ClientType { Name = "B2C", Code = "2", Description = "Business to Consumer", BusinessId = businessId },
                new ClientType { Name = "Regierung", Code = "GOV", Description = "Government agencies", BusinessId = businessId },
                new ClientType { Name = "NGO", Code = "NGO", Description = "Non-profit organizations", BusinessId = businessId },
                new ClientType { Name = "Einzelhandel", Code = "RETAIL", Description = "Retail customers", BusinessId = businessId },
                new ClientType { Name = "Großhandel", Code = "WHSALE", Description = "Wholesale buyers", BusinessId = businessId },
                new ClientType { Name = "Wiederverkäufer", Code = "RESELLER", Description = "Authorized resellers", BusinessId = businessId }
            };
            await context.ClientTypes.AddRangeAsync(clientTypes);
            await context.SaveChangesAsync();
        }

        // Seed TaxRates - Will be populated by VatRateUpdateService from Vatlayer API
        // Seed only Austria rates initially (for backward compatibility)
        if (!await context.TaxRates.IgnoreQueryFilters().AnyAsync())
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
        var domesticArea = await context.ClientAreas.IgnoreQueryFilters().FirstOrDefaultAsync(ca => ca.Code == "1");
        var standardTaxRate = await context.TaxRates.IgnoreQueryFilters().FirstOrDefaultAsync(tr => tr.Rate == 20m);
        var reducedTaxRate10 = await context.TaxRates.IgnoreQueryFilters().FirstOrDefaultAsync(tr => tr.Rate == 10m);
        var reducedTaxRate13 = await context.TaxRates.IgnoreQueryFilters().FirstOrDefaultAsync(tr => tr.Rate == 13m);
        var zeroTaxRate = await context.TaxRates.IgnoreQueryFilters().FirstOrDefaultAsync(tr => tr.Rate == 0m);

        if (!await context.Accounts.IgnoreQueryFilters().AnyAsync() && domesticArea != null && standardTaxRate != null)
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
        if (!await context.Taxes.IgnoreQueryFilters().AnyAsync())
        {
            var accounts = await context.Accounts.IgnoreQueryFilters().Include(a => a.DefaultTaxRate).ToListAsync();
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
        if (!await context.Units.IgnoreQueryFilters().AnyAsync())
        {
            var units = new[]
            {
                new Unit { Name = "Stück", ShortName = "Stk", BusinessId = businessId },
                new Unit { Name = "KG", ShortName = "kg", BusinessId = businessId },
                new Unit { Name = "Karton", ShortName = "Karton", BusinessId = businessId },
                new Unit { Name = "Liter", ShortName = "l", BusinessId = businessId },
                new Unit { Name = "Meter", ShortName = "m", BusinessId = businessId },
                new Unit { Name = "Pauschalbetrag", ShortName = "Pauschal", BusinessId = businessId },
                new Unit { Name = "Tonne", ShortName = "t", BusinessId = businessId },
                new Unit { Name = "Quadratmeter", ShortName = "m²", BusinessId = businessId },
                new Unit { Name = "Kubikmeter", ShortName = "m³", BusinessId = businessId },
                new Unit { Name = "Stunden", ShortName = "h", BusinessId = businessId },
                new Unit { Name = "Tage", ShortName = "day", BusinessId = businessId },
                new Unit { Name = "Box", ShortName = "box", BusinessId = businessId },
                new Unit { Name = "Palette", ShortName = "pal", BusinessId = businessId },
                new Unit { Name = "Set", ShortName = "set", BusinessId = businessId },
                new Unit { Name = "Pieces", ShortName = "pcs", BusinessId = businessId }
            };
            await context.Units.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }

        // Seed Currencies
        if (!await context.Currencies.IgnoreQueryFilters().AnyAsync())
        {
            var currencies = new[]
            {
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRate = 1.0m, IsDefault = true, BusinessId = businessId },
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", ExchangeRate = 1.1m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", ExchangeRate = 1.05m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRate = 1.17m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "RUB", Name = "Russian Ruble", Symbol = "₽", ExchangeRate = 0.01m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "PLN", Name = "Polish Zloty", Symbol = "zł", ExchangeRate = 0.25m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "CZK", Name = "Czech Koruna", Symbol = "Kč", ExchangeRate = 0.045m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "HUF", Name = "Hungarian Forint", Symbol = "Ft", ExchangeRate = 0.0028m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", ExchangeRate = 0.1m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "NOK", Name = "Norwegian Krone", Symbol = "kr", ExchangeRate = 0.095m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "DKK", Name = "Danish Krone", Symbol = "kr", ExchangeRate = 0.135m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", ExchangeRate = 0.0075m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", ExchangeRate = 0.15m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", ExchangeRate = 0.72m, IsDefault = false, BusinessId = businessId },
                new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", ExchangeRate = 0.8m, IsDefault = false, BusinessId = businessId }
            };
            await context.Currencies.AddRangeAsync(currencies);
            await context.SaveChangesAsync();
        }

        // Seed PaymentMethods
        if (!await context.PaymentMethods.IgnoreQueryFilters().AnyAsync())
        {
            var paymentMethods = new[]
            {
                new PaymentMethod { Name = "Überweisung", IsDefault = true, BusinessId = businessId },
                new PaymentMethod { Name = "Barzahlung", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Kreditkarte", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Debitkarte", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "PayPal", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Lastschrift (SEPA)", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Scheck", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Rechnung (30 Tage)", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Rechnung (14 Tage)", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Vorkasse", IsDefault = false, BusinessId = businessId },
                new PaymentMethod { Name = "Nachnahme", IsDefault = false, BusinessId = businessId }
            };
            await context.PaymentMethods.AddRangeAsync(paymentMethods);
            await context.SaveChangesAsync();
        }

        // Seed Clients (Kunden)
        await SeedClientsAsync(context, businessId);
    }

    /// <summary>
    /// Import clients from CSV or create basic test clients
    /// </summary>
    private static async Task SeedClientsAsync(ApplicationDbContext context, int businessId)
    {
        if (await context.Clients.IgnoreQueryFilters().AnyAsync(c => !c.IsDeleted))
            return;

        var b2bType = await context.ClientTypes.IgnoreQueryFilters().FirstOrDefaultAsync(ct => ct.Name == "B2B");
        var b2cType = await context.ClientTypes.IgnoreQueryFilters().FirstOrDefaultAsync(ct => ct.Name == "B2C");
        var domesticArea = await context.ClientAreas.IgnoreQueryFilters().FirstOrDefaultAsync(ca => ca.Code == "1");

        var clients = new List<Client>
        {
            // From Invoice examples in database
            new Client
            {
                CompanyName = "ANDREI GIGI",
                ContactPerson = "Andrei Gigi",
                Country = "Austria",
                BusinessId = businessId,
                ClientTypeId = b2bType?.Id,
                ClientAreaId = domesticArea?.Id,
                VatNumber = "ATU12345678",
                PaymentTermsDays = 30
            },
            new Client
            {
                CompanyName = "ALEMIRA GROUP, s.r.o.",
                ContactPerson = "Aleksandr Alekseev",
                Country = "Slovakia",
                City = "Bratislava",
                BusinessId = businessId,
                ClientTypeId = b2bType?.Id,
                ClientAreaId = domesticArea?.Id,
                VatNumber = "SK2121212121",
                PaymentTermsDays = 45
            },
            new Client
            {
                CompanyName = "ALERO Handels GmbH",
                ContactPerson = "Contact Person",
                Country = "Austria",
                City = "Vienna",
                BusinessId = businessId,
                ClientTypeId = b2bType?.Id,
                ClientAreaId = domesticArea?.Id,
                VatNumber = "ATU98765432",
                PaymentTermsDays = 30
            },
            new Client
            {
                CompanyName = "Test Client B2C",
                ContactPerson = "Test Person",
                Country = "Austria",
                City = "Vienna",
                Email = "test@example.at",
                BusinessId = businessId,
                ClientTypeId = b2cType?.Id,
                ClientAreaId = domesticArea?.Id,
                PaymentTermsDays = 7
            }
        };

        await context.Clients.AddRangeAsync(clients);
        await context.SaveChangesAsync();
    }
}
