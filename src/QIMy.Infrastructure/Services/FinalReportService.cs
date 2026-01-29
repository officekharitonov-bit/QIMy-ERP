using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;
using System.Globalization;
using System.Text;

namespace QIMy.Infrastructure.Services;

public class FinalReportService
{
    private readonly ApplicationDbContext _context;

    public FinalReportService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generate a comprehensive financial report (Invoice Summary)
    /// </summary>
    public async Task<byte[]> GenerateFinalReportPdfAsync(int businessId, DateTime fromDate, DateTime toDate)
    {
        var invoices = await _context.Invoices
            .Where(i => i.BusinessId == businessId && !i.IsDeleted &&
                   i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
            .Include(i => i.Client)
            .Include(i => i.Items)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginVertical(20);
                page.MarginHorizontal(30);

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("FINAL REPORT").FontSize(24).Bold();
                            c.Item().PaddingTop(5).Text($"{business?.Name ?? "Company"}").FontSize(12);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"From: {fromDate:dd.MM.yyyy}").FontSize(10);
                            c.Item().Text($"Till: {toDate:dd.MM.yyyy}").FontSize(10);
                            c.Item().PaddingTop(5).Text($"Generated: {DateTime.UtcNow:dd.MM.yyyy HH:mm}").FontSize(9).Italic();
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(10).LineHorizontal(1);

                    // Summary Section
                    col.Item().PaddingTop(10).Column(summary =>
                    {
                        summary.Item().Text("SUMMARY").FontSize(14).Bold();
                        summary.Item().PaddingTop(10).Row(summaryRow =>
                        {
                            var totalAmount = invoices.Sum(i => i.TotalAmount);
                            var totalTax = invoices.Sum(i => i.TaxAmount);
                            var totalItems = invoices.Sum(i => i.Items.Count);

                            summaryRow.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Total Invoices:").Bold();
                                c.Item().Text(invoices.Count.ToString()).FontSize(14).Bold();
                            });

                            summaryRow.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Total Amount (Gross):").Bold();
                                c.Item().Text($"{totalAmount:N2} EUR").FontSize(14).Bold();
                            });

                            summaryRow.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Total Tax:").Bold();
                                c.Item().Text($"{totalTax:N2} EUR").FontSize(14).Bold();
                            });

                            summaryRow.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Total Items:").Bold();
                                c.Item().Text(totalItems.ToString()).FontSize(14).Bold();
                            });
                        });
                    });

                    col.Item().PaddingTop(20).LineHorizontal(1);

                    // Detailed Invoice Table
                    if (invoices.Any())
                    {
                        col.Item().PaddingTop(15).Column(table =>
                        {
                            table.Item().Text("INVOICE DETAILS").FontSize(12).Bold();

                            table.Item().PaddingTop(10).Table(t =>
                            {
                                // Header row
                                t.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1f);  // Invoice #
                                    columns.RelativeColumn(1.5f); // Client
                                    columns.RelativeColumn(1f);   // Date
                                    columns.RelativeColumn(1f);   // Items
                                    columns.RelativeColumn(1.5f); // Amount
                                });

                                t.Header(header =>
                                {
                                    header.Cell().Background("#E8E8E8").Padding(5).Text("Invoice #").Bold();
                                    header.Cell().Background("#E8E8E8").Padding(5).Text("Client").Bold();
                                    header.Cell().Background("#E8E8E8").Padding(5).Text("Date").Bold();
                                    header.Cell().Background("#E8E8E8").Padding(5).AlignRight().Text("Items").Bold();
                                    header.Cell().Background("#E8E8E8").Padding(5).AlignRight().Text("Total Amount").Bold();
                                });

                                // Data rows
                                foreach (var invoice in invoices)
                                {
                                    t.Cell().Padding(5).Text(invoice.InvoiceNumber ?? "N/A");
                                    t.Cell().Padding(5).Text(invoice.Client?.CompanyName ?? "Unknown");
                                    t.Cell().Padding(5).Text(invoice.InvoiceDate.ToString("dd.MM.yyyy"));
                                    t.Cell().Padding(5).AlignRight().Text(invoice.Items.Count.ToString());
                                    t.Cell().Padding(5).AlignRight().Text($"{invoice.TotalAmount:N2} EUR");
                                }
                            });
                        });
                    }

                    // Footer
                    col.Item().PaddingTop(20).LineHorizontal(1);
                    col.Item().PaddingTop(10).Text($"Report generated by QIMy ERP").FontSize(8).Italic();
                });
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Generate CSV export of invoices in BMD NTCS format (29 fields)
    /// Uses German date and number formatting for Austrian accounting systems
    /// </summary>
    public async Task<string> GenerateFinalReportCsvAsync(int businessId, DateTime fromDate, DateTime toDate)
    {
        var invoices = await _context.Invoices
            .Where(i => i.BusinessId == businessId && !i.IsDeleted &&
                   i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
            .Include(i => i.Client)
            .Include(i => i.Items)
                .ThenInclude(item => item.Tax!)
                    .ThenInclude(tax => tax.Account)
            .Include(i => i.Currency)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);
        
        // German culture for number and date formatting
        var germanCulture = CultureInfo.GetCultureInfo("de-DE");
        
        var csv = new StringBuilder();
        
        // BMD NTCS header (29 fields)
        csv.AppendLine("satzart;konto;gkonto;buchdatum;belegdatum;belegnr;betrag;steuer;text;buchtyp;buchsymbol;filiale;prozent;steuercode;buchcode;fwbetrag;fwsteuer;waehrung;periode;gegenbuchkz;verbuchkz;ausz-belegnr;ausz-betrag;extid;extid;verbuchstatus;uidnr;dokumente");

        foreach (var invoice in invoices)
        {
            // Extract data with safe defaults
            var clientCode = invoice.Client?.ClientCode?.ToString() ?? "";
            var clientName = invoice.Client?.CompanyName ?? "Unknown";
            var vatNumber = invoice.Client?.VatNumber ?? "";
            var invoiceNumber = invoice.InvoiceNumber ?? "";
            var currencyCode = invoice.Currency?.Code ?? "EUR";
            
            // Determine revenue account (gkonto) from first invoice item
            var firstItem = invoice.Items.FirstOrDefault();
            var revenueAccount = firstItem?.Tax?.Account?.AccountNumber ?? "4000";
            
            // Determine steuercode (tax code) based on invoice type
            var steuercode = GetSteuercodeForInvoiceType(invoice);
            
            // Calculate amounts
            var subTotal = invoice.SubTotal; // Net amount (Netto)
            var taxAmount = invoice.TaxAmount;
            var totalAmount = invoice.TotalAmount; // Gross amount (Brutto)
            
            // Tax percentage (from first item or invoice)
            var taxPercent = invoice.Proz ?? 20.0m;
            
            // Format dates as German format (dd.MM.yyyy)
            var buchdatum = invoice.InvoiceDate.ToString("dd.MM.yyyy", germanCulture);
            var belegdatum = invoice.InvoiceDate.ToString("dd.MM.yyyy", germanCulture);
            
            // Format numbers as German format (comma as decimal separator)
            var betragStr = subTotal.ToString("N2", germanCulture);
            var steuerStr = taxAmount.ToString("N2", germanCulture);
            var fwbetragStr = ""; // Foreign currency amount (empty if EUR)
            var fwsteuerStr = ""; // Foreign currency tax (empty if EUR)
            
            // Period (month as 2 digits)
            var periode = invoice.InvoiceDate.ToString("MM", CultureInfo.InvariantCulture);
            
            // Invoice type symbol (AR = Ausgangsrechnung)
            var buchsymbol = "AR";
            
            // Text field: Full invoice description
            var text = $"INVOICE {buchsymbol}{invoiceNumber} {clientName}, {vatNumber}";
            
            // Build CSV row (29 fields)
            csv.Append("0;"); // satzart (always 0)
            csv.Append($"{clientCode};"); // konto (client account number)
            csv.Append($"{revenueAccount};"); // gkonto (revenue account)
            csv.Append($"{buchdatum};"); // buchdatum (posting date)
            csv.Append($"{belegdatum};"); // belegdatum (document date)
            csv.Append($"{invoiceNumber.Replace(buchsymbol, "")};"); // belegnr (document number without AR prefix)
            csv.Append($"{betragStr};"); // betrag (net amount)
            csv.Append($"{steuerStr};"); // steuer (tax amount)
            csv.Append($"{text};"); // text (description)
            csv.Append("1;"); // buchtyp (1=AR, 2=ER)
            csv.Append($"{buchsymbol};"); // buchsymbol (AR/ER/etc)
            csv.Append(";"); // filiale (branch - empty)
            csv.Append($"{taxPercent.ToString("N1", germanCulture).Replace(",", "")};"); // prozent (tax percent without decimal separator)
            csv.Append($"{steuercode};"); // steuercode (1-99)
            csv.Append("1;"); // buchcode (1=normal)
            csv.Append($"{fwbetragStr};"); // fwbetrag (foreign currency amount)
            csv.Append($"{fwsteuerStr};"); // fwsteuer (foreign currency tax)
            csv.Append($"{currencyCode};"); // waehrung (currency)
            csv.Append($"{periode};"); // periode (month)
            csv.Append("E;"); // gegenbuchkz (E=Einzelposten)
            csv.Append("A;"); // verbuchkz (A=Automatik)
            csv.Append(";"); // ausz-belegnr (payment doc number - empty)
            csv.Append(";"); // ausz-betrag (payment amount - empty)
            csv.Append(";"); // extid (external ID 1 - empty)
            csv.Append(";"); // extid (external ID 2 - empty)
            csv.Append("0;"); // verbuchstatus (0=nicht gebucht)
            csv.Append($"{vatNumber};"); // uidnr (VAT number)
            csv.AppendLine(";"); // dokumente (documents - empty)
        }

        return csv.ToString();
    }
    
    /// <summary>
    /// Determine Austrian tax code (Steuercode) based on invoice type
    /// </summary>
    private int GetSteuercodeForInvoiceType(Invoice invoice)
    {
        return invoice.InvoiceType switch
        {
            InvoiceType.Domestic => 1, // Standard 20% VAT
            InvoiceType.Export => 51, // Tax-free export (0%)
            InvoiceType.IntraEUSale => 77, // Intra-EU supply (0%)
            InvoiceType.ReverseCharge => 88, // Reverse charge
            InvoiceType.SmallBusinessExemption => 62, // Kleinunternehmer
            InvoiceType.TriangularTransaction => 78, // Dreiecksgeschäft
            _ => invoice.Steuercode ?? 1 // Use invoice's Steuercode or default to 1
        };
    }

    /// <summary>
    /// Generate Excel export (requires EPPlus)
    /// For now, returns CSV - Excel can be added later
    /// </summary>
    public async Task<string> GenerateFinalReportExcelAsync(int businessId, DateTime fromDate, DateTime toDate)
    {
        // TODO: Implement Excel generation with EPPlus
        return await GenerateFinalReportCsvAsync(businessId, fromDate, toDate);
    }
}
