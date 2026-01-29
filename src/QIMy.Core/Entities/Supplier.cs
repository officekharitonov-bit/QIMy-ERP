using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Supplier/Vendor entity for incoming invoices
/// </summary>
public class Supplier : BaseEntity, IMustHaveBusiness
{
    public int BusinessId { get; set; }
    public int SupplierCode { get; set; } // 300000-399999
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? BankAccount { get; set; }

    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<ExpenseInvoice> ExpenseInvoices { get; set; } = new List<ExpenseInvoice>();
}
