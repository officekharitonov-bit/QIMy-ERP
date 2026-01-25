using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

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
    /// Generate CSV export of invoices
    /// </summary>
    public async Task<string> GenerateFinalReportCsvAsync(int businessId, DateTime fromDate, DateTime toDate)
    {
        var invoices = await _context.Invoices
            .Where(i => i.BusinessId == businessId && !i.IsDeleted &&
                   i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
            .Include(i => i.Client)
            .Include(i => i.Items)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Invoice Number;Client Name;Invoice Date;Due Date;Item Count;Sub Total;Tax Amount;Total Amount;Status");

        foreach (var invoice in invoices)
        {
            var clientName = (invoice.Client?.CompanyName ?? "Unknown").Replace(";", ",");
            csv.AppendLine($"\"{invoice.InvoiceNumber}\";\"{clientName}\";{invoice.InvoiceDate:yyyy-MM-dd};{invoice.DueDate:yyyy-MM-dd};{invoice.Items.Count};{invoice.SubTotal:F2};{invoice.TaxAmount:F2};{invoice.TotalAmount:F2};{invoice.Status}");
        }

        return csv.ToString();
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
