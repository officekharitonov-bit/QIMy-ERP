namespace QIMy.Core.DTOs;

public class ClientImportDto
{
    public string? CountryCode { get; set; }
    public string? ClientCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Currency { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DiscountPercent { get; set; }
    public string? DiscountDays { get; set; }
    public string? VatNumber { get; set; }
    public string? AccountNumber { get; set; } // Erlöskonto / Kunden-Vorschlag Gegenkonto
    public string? SupplierSuggestedAccount { get; set; } // Lief-Vorschlag Gegenkonto
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? TaxNumber { get; set; }
    public string? ExternalAccountNumber { get; set; } // Freifeld 01 / Externe KontoNr
    public string? Branch { get; set; } // Filiale
    public string? CountryNumber { get; set; } // Land-Nr
    public string? Description { get; set; } // Waren/Dienstleistungsbeschreibung
    public string? FreeField11 { get; set; }
    public string? FreeField04 { get; set; }
    public string? FreeField05 { get; set; }
    public string? FreeField02 { get; set; }
    public string? FreeField03 { get; set; }
    public int RowNumber { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public bool IsValid => !ValidationErrors.Any();
}

public class ImportResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int SkippedCount { get; set; }
    public List<ImportError> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime ImportedAt { get; set; }
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string? ClientCode { get; set; }
    public string? CompanyName { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string[] Details { get; set; } = Array.Empty<string>();
}
