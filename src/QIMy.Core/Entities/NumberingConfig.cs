using QIMy.Core.Enums;

namespace QIMy.Core.Entities;

/// <summary>
/// Numbering configuration for different document types
/// Defines how document numbers are generated (format, prefix, next number)
/// </summary>
public class NumberingConfig : BaseEntity
{
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    
    /// <summary>Document type (Invoice, Quote, Return, etc.)</summary>
    public DocumentTypeEnum DocumentType { get; set; }

    /// <summary>Numbering format pattern</summary>
    public NumberingFormatEnum Format { get; set; } = NumberingFormatEnum.YearDashNumber;

    /// <summary>Custom prefix (e.g., "INV", "RFQ", "STORNO")</summary>
    public string? Prefix { get; set; }

    /// <summary>Separator between parts (e.g., "-", "/", ".")</summary>
    public string Separator { get; set; } = "-";

    /// <summary>Number of digits for the sequential number (2-5)</summary>
    public int NumberLength { get; set; } = 3;

    /// <summary>Next sequential number to use</summary>
    public long NextNumber { get; set; } = 1;

    /// <summary>Year for yearly reset (0 = no reset, current year = yearly reset)</summary>
    public int? ResetYear { get; set; }

    /// <summary>Example of generated number</summary>
    public string? ExampleNumber { get; set; }

    /// <summary>Is this configuration active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }
}
