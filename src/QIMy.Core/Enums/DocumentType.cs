namespace QIMy.Core.Enums;

/// <summary>
/// Document types - Типы документов
/// AR - Ausgangsrechnungen (Sales Invoices)
/// ER - Eingangsrechnungen (Purchase Invoices)
/// AG - Angebote (Quotes/Offers)
/// ST - Storno (Returns/Reversals)
/// </summary>
public enum DocumentTypeEnum
{
    Invoice = 0,          // Ausgangsrechnung (AR)
    ExpenseInvoice = 1,   // Eingangsrechnung (ER)
    Quote = 2,            // Angebot (AG) - Quotation
    Return = 3            // Storno (ST) - Return/Credit note
}
