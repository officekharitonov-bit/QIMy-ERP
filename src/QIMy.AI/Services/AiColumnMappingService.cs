using System.Reflection;
using System.Text.RegularExpressions;
using FuzzySharp;

namespace QIMy.AI.Services;

/// <summary>
/// AI-powered smart column mapping implementation using fuzzy matching
/// </summary>
public class AiColumnMappingService : IAiColumnMappingService
{
    private readonly Dictionary<string, string[]> _commonAliases = new()
    {
        // Client/Supplier common aliases
        { "CompanyName", new[] { "company", "name", "firma", "firmenname", "bezeichnung", "title" } },
        { "VatNumber", new[] { "vat", "uid", "ust", "umsatzsteuer", "tax", "taxid", "vatnumber", "uidnummer" } },
        { "Email", new[] { "email", "mail", "e-mail", "emailaddress" } },
        { "Phone", new[] { "phone", "tel", "telefon", "telephone", "phonenumber" } },
        { "Street", new[] { "street", "strasse", "straße", "address", "adresse", "street1" } },
        { "City", new[] { "city", "stadt", "ort", "place" } },
        { "PostalCode", new[] { "zip", "postal", "postcode", "plz", "postalcode", "zipcode" } },
        { "Country", new[] { "country", "land", "nation", "countrycode" } },
        { "ContactPerson", new[] { "contact", "person", "ansprechpartner", "contactperson", "kontakt" } },
        { "Website", new[] { "website", "web", "url", "homepage", "site" } },
        { "BankAccount", new[] { "iban", "account", "kontonummer", "bankaccount", "accountnumber" } },
        { "BankName", new[] { "bank", "bankname", "bankhaus", "kreditinstitut" } },
        { "TaxOffice", new[] { "finanzamt", "taxoffice", "steueramt" } },
        { "CommercialRegister", new[] { "fn", "firmenbuchnummer", "hrb", "commercialregister", "register" } },
        { "ClientCode", new[] { "code", "kundennummer", "clientcode", "kundencode", "id", "nummer" } },
        { "ClientType", new[] { "type", "typ", "clienttype", "kundentyp", "category" } },
        { "ClientArea", new[] { "area", "bereich", "region", "zone", "gebiet" } },

        // Invoice common aliases
        { "InvoiceNumber", new[] { "invoice", "number", "rechnungsnummer", "invoicenumber", "nr", "nummer" } },
        { "InvoiceDate", new[] { "date", "datum", "invoicedate", "rechnungsdatum", "created" } },
        { "DueDate", new[] { "due", "fälligkeit", "duedate", "paymentdue", "zahlungsziel" } },
        { "Amount", new[] { "amount", "betrag", "total", "sum", "summe", "value" } },
        { "VatAmount", new[] { "vat", "ust", "tax", "steuer", "vatamount", "taxamount" } },
        { "Description", new[] { "description", "beschreibung", "text", "details", "bemerkung", "notes" } },

        // Common fields
        { "CreatedDate", new[] { "created", "createdat", "angelegt", "erstellt", "datum" } },
        { "Notes", new[] { "notes", "note", "notiz", "notizen", "bemerkung", "comment" } },
        { "IsActive", new[] { "active", "aktiv", "status", "enabled" } }
    };

    public Task<ColumnMappingResult> MapColumnsAsync<TEntity>(
        string[] csvHeaders,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return MapColumnsWithSampleDataAsync<TEntity>(csvHeaders, new List<string[]>(), cancellationToken);
    }

    public Task<ColumnMappingResult> MapColumnsWithSampleDataAsync<TEntity>(
        string[] csvHeaders,
        List<string[]> sampleRows,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        var result = new ColumnMappingResult();
        var entityProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        var usedProperties = new HashSet<string>();

        // Step 1: Exact matches (case-insensitive)
        for (int i = 0; i < csvHeaders.Length; i++)
        {
            var header = CleanHeader(csvHeaders[i]);
            var exactMatch = entityProperties.FirstOrDefault(p =>
                string.Equals(p.Name, header, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null && !usedProperties.Contains(exactMatch.Name))
            {
                result.Mappings[i] = exactMatch.Name;
                result.Confidences[i] = 1.0m; // 100% confidence for exact match
                usedProperties.Add(exactMatch.Name);
            }
        }

        // Step 2: Fuzzy matching with aliases
        for (int i = 0; i < csvHeaders.Length; i++)
        {
            if (result.Mappings.ContainsKey(i))
                continue; // Already mapped

            var header = CleanHeader(csvHeaders[i]);
            var bestMatch = FindBestMatch(header, entityProperties, usedProperties, sampleRows.Count > 0 ? sampleRows.Select(r => r.ElementAtOrDefault(i)).ToList() : null);

            if (bestMatch != null)
            {
                result.Mappings[i] = bestMatch.PropertyName;
                result.Confidences[i] = bestMatch.Confidence;
                usedProperties.Add(bestMatch.PropertyName);

                if (bestMatch.Confidence < 0.7m)
                {
                    result.Warnings.Add($"Low confidence mapping: '{csvHeaders[i]}' → '{bestMatch.PropertyName}' ({bestMatch.Confidence:P0})");
                }
            }
            else
            {
                result.UnmappedColumns.Add(new UnmappedColumn
                {
                    ColumnIndex = i,
                    ColumnName = csvHeaders[i]
                });
            }
        }

        // Step 3: Find unmapped required properties
        var requiredProperties = entityProperties
            .Where(p => IsRequired(p) && !usedProperties.Contains(p.Name))
            .Select(p => p.Name)
            .ToList();

        result.UnmappedProperties.AddRange(requiredProperties);

        if (requiredProperties.Any())
        {
            result.Warnings.Add($"Missing required fields: {string.Join(", ", requiredProperties)}");
        }

        // Step 4: Calculate overall confidence
        if (result.Mappings.Any())
        {
            result.OverallConfidence = result.Confidences.Values.Average();
        }
        else
        {
            result.OverallConfidence = 0m;
            result.Warnings.Add("No columns could be mapped automatically");
        }

        return Task.FromResult(result);
    }

    private PropertyMatch? FindBestMatch(
        string header,
        List<PropertyInfo> properties,
        HashSet<string> usedProperties,
        List<string?>? sampleData)
    {
        PropertyMatch? bestMatch = null;
        int bestScore = 0;

        foreach (var property in properties)
        {
            if (usedProperties.Contains(property.Name))
                continue;

            // Check direct property name match
            var directScore = Fuzz.Ratio(header.ToLowerInvariant(), property.Name.ToLowerInvariant());

            // Check aliases
            int aliasScore = 0;
            if (_commonAliases.TryGetValue(property.Name, out var aliases))
            {
                aliasScore = aliases.Max(alias => Fuzz.Ratio(header.ToLowerInvariant(), alias.ToLowerInvariant()));
            }

            // Sample data validation (if available)
            decimal dataConfidence = 1.0m;
            if (sampleData != null && sampleData.Any(d => !string.IsNullOrWhiteSpace(d)))
            {
                dataConfidence = ValidateDataType(property.PropertyType, sampleData);
            }

            // Use best of direct or alias score
            var fuzzyScore = Math.Max(directScore, aliasScore);

            if (fuzzyScore > bestScore && fuzzyScore >= 60) // Minimum 60% similarity
            {
                bestScore = fuzzyScore;
                bestMatch = new PropertyMatch
                {
                    PropertyName = property.Name,
                    Confidence = (fuzzyScore / 100m) * dataConfidence, // Combine fuzzy score with data validation
                    FuzzyScore = fuzzyScore
                };
            }
        }

        return bestMatch;
    }

    private decimal ValidateDataType(Type propertyType, List<string?> sampleData)
    {
        var validData = sampleData.Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
        if (!validData.Any())
            return 1.0m; // No data to validate

        int validCount = 0;
        int totalCount = validData.Count;

        foreach (var data in validData)
        {
            if (string.IsNullOrWhiteSpace(data))
                continue;

            bool isValid = propertyType.Name switch
            {
                "Int32" or "Int64" => int.TryParse(data, out _) || long.TryParse(data, out _),
                "Decimal" or "Double" => decimal.TryParse(data.Replace(',', '.'), out _),
                "DateTime" => DateTime.TryParse(data, out _),
                "Boolean" => bool.TryParse(data, out _) || data == "0" || data == "1",
                "String" => true, // String always valid
                _ => true // Unknown types treated as valid
            };

            if (isValid)
                validCount++;
        }

        return totalCount > 0 ? (decimal)validCount / totalCount : 1.0m;
    }

    private bool IsRequired(PropertyInfo property)
    {
        // Check for Required attribute or non-nullable reference types
        var requiredAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
        if (requiredAttr.Any())
            return true;

        // Check if property type is non-nullable value type
        var propertyType = property.PropertyType;
        if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
        {
            // It's a non-nullable value type, but we'll be lenient here
            // Only mark as truly required if it has Required attribute
            return false;
        }

        return false;
    }

    private string CleanHeader(string header)
    {
        // Remove special characters, whitespace, quotes
        var cleaned = Regex.Replace(header, @"[^\w\säöüÄÖÜß]", "");
        cleaned = Regex.Replace(cleaned, @"\s+", "");
        return cleaned.Trim();
    }
}

internal class PropertyMatch
{
    public string PropertyName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public int FuzzyScore { get; set; }
}
