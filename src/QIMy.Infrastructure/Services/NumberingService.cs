using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Enums;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for generating document numbers based on configured patterns
/// </summary>
public class NumberingService
{
    private readonly ApplicationDbContext _context;

    public NumberingService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Generate next document number based on configuration
    /// </summary>
    public async Task<string> GenerateNextNumberAsync(DocumentTypeEnum documentType)
    {
        var config = await _context.Set<NumberingConfig>()
            .FirstOrDefaultAsync(c => c.DocumentType == documentType && c.IsActive);

        if (config == null)
        {
            throw new InvalidOperationException($"No active numbering configuration found for document type: {documentType}");
        }

        // Check if yearly reset is needed
        if (config.ResetYear.HasValue && config.ResetYear != DateTime.Now.Year)
        {
            config.NextNumber = 1;
            config.ResetYear = DateTime.Now.Year;
        }

        string number = GenerateNumberFromConfig(config);

        // Increment for next time
        config.NextNumber++;
        config.ExampleNumber = number;

        await _context.SaveChangesAsync();

        return number;
    }

    /// <summary>
    /// Generate a preview of the document number without incrementing
    /// </summary>
    public string PreviewNumber(NumberingConfig config)
    {
        return GenerateNumberFromConfig(config);
    }

    private string GenerateNumberFromConfig(NumberingConfig config)
    {
        string numberPart = config.NextNumber.ToString().PadLeft(config.NumberLength, '0');
        string yearPart = DateTime.Now.Year.ToString();
        string yearShortPart = DateTime.Now.Year.ToString().Substring(2);

        string number = config.Format switch
        {
            NumberingFormatEnum.YearDashNumber => $"{yearPart}{config.Separator}{numberPart}",
            NumberingFormatEnum.YearNumber => $"{yearPart}{numberPart}",
            NumberingFormatEnum.YearShortDashNumber => $"{yearShortPart}{config.Separator}{numberPart}",
            NumberingFormatEnum.YearShortNumber => $"{yearShortPart}{numberPart}",
            NumberingFormatEnum.NumberOnly => numberPart,
            NumberingFormatEnum.YearSlashNumber => $"{yearPart}{config.Separator}{numberPart}",
            NumberingFormatEnum.Custom => GenerateCustomFormat(config, numberPart, yearPart, yearShortPart),
            _ => $"{yearPart}{config.Separator}{numberPart}"
        };

        // Add prefix if configured
        if (!string.IsNullOrEmpty(config.Prefix))
        {
            number = $"{config.Prefix}{config.Separator}{number}";
        }

        return number;
    }

    private string GenerateCustomFormat(NumberingConfig config, string numberPart, string yearPart, string yearShortPart)
    {
        // Custom format pattern support: {YYYY}, {YY}, {NNN}, {PREFIX}
        if (string.IsNullOrEmpty(config.Prefix))
            return $"{yearPart}{config.Separator}{numberPart}";

        return config.Prefix
            .Replace("{YYYY}", yearPart)
            .Replace("{YY}", yearShortPart)
            .Replace("{NNN}", numberPart)
            .Replace("{SEP}", config.Separator);
    }

    /// <summary>
    /// Get or create default numbering configuration for a document type
    /// </summary>
    public async Task<NumberingConfig> GetOrCreateDefaultAsync(DocumentTypeEnum documentType)
    {
        var config = await _context.Set<NumberingConfig>()
            .FirstOrDefaultAsync(c => c.DocumentType == documentType && c.IsActive);

        if (config != null)
            return config;

        // Create default configuration
        config = new NumberingConfig
        {
            DocumentType = documentType,
            Format = NumberingFormatEnum.YearDashNumber,
            Prefix = GetDefaultPrefix(documentType),
            Separator = "-",
            NumberLength = 3,
            NextNumber = 1,
            ResetYear = DateTime.Now.Year,
            IsActive = true,
            Notes = $"Default configuration for {documentType}"
        };

        config.ExampleNumber = PreviewNumber(config);

        _context.Set<NumberingConfig>().Add(config);
        await _context.SaveChangesAsync();

        return config;
    }

    private string GetDefaultPrefix(DocumentTypeEnum documentType) => documentType switch
    {
        DocumentTypeEnum.Invoice => "INV",
        DocumentTypeEnum.ExpenseInvoice => "EXP",
        DocumentTypeEnum.Quote => "RFQ",
        DocumentTypeEnum.Return => "STORNO",
        _ => "DOC"
    };
}
