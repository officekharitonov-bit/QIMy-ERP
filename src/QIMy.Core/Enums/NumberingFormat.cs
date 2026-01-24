namespace QIMy.Core.Enums;

/// <summary>
/// Numbering format patterns
/// Examples:
/// - YYYY-NNN: 2026-001
/// - YYYYNNN: 2026001
/// - YY-NNN: 26-001
/// - YYNNN: 26001
/// - NNN: 001
/// - YYYY/NNN: 2026/001
/// - DOC-YYYY-NNN: INV-2026-001
/// </summary>
public enum NumberingFormatEnum
{
    /// <summary>Year-Number (2026-001)</summary>
    YearDashNumber = 0,
    
    /// <summary>YearNumber (2026001)</summary>
    YearNumber = 1,
    
    /// <summary>2-digit Year-Number (26-001)</summary>
    YearShortDashNumber = 2,
    
    /// <summary>2-digit YearNumber (26001)</summary>
    YearShortNumber = 3,
    
    /// <summary>Number only (001)</summary>
    NumberOnly = 4,
    
    /// <summary>Year/Number (2026/001)</summary>
    YearSlashNumber = 5,
    
    /// <summary>Custom format with prefix and pattern</summary>
    Custom = 6
}
