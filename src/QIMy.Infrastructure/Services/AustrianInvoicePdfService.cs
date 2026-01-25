using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for generating invoices according to Austrian legal requirements (Rechnungsmerkmale)
/// </summary>
public class AustrianInvoicePdfService
{
    private readonly ApplicationDbContext _context;

    public AustrianInvoicePdfService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generate a single invoice in Austrian format
    /// </summary>
    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Business)
            .Include(i => i.Currency)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {invoiceId} not found");

        return GenerateInvoicePdf(invoice);
    }

    private byte[] GenerateInvoicePdf(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginVertical(15);
                page.MarginHorizontal(20);

                page.Content().Column(col =>
                {
                    // Invoice header with type indicator
                    col.Item().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("RECHNUNG").FontSize(20).Bold();
                                c.Item().PaddingTop(5).Text(GetInvoiceTypeLabel(invoice.InvoiceType)).FontSize(10).Italic().FontColor("666666");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text($"Rechnungsnummer: {invoice.InvoiceNumber}").FontSize(10).Bold();
                                c.Item().Text($"Datum: {invoice.InvoiceDate:dd.MM.yyyy}").FontSize(10);
                                c.Item().Text($"Fälligkeit: {invoice.DueDate:dd.MM.yyyy}").FontSize(10);
                            });
                        });
                    });

                    col.Item().PaddingTop(15).LineHorizontal(1);

                    // Seller and Buyer Information
                    col.Item().PaddingTop(15).Row(row =>
                    {
                        // Seller (left)
                        row.RelativeItem().Column(seller =>
                        {
                            seller.Item().Text("Rechnungssteller (Verkäufer):").Bold().FontSize(9);
                            seller.Item().PaddingTop(5).Column(c =>
                            {
                                c.Item().Text(invoice.Business?.Name ?? "Unbekannt").FontSize(10).Bold();
                                c.Item().Text(invoice.Business?.LegalName ?? "").FontSize(9);
                                if (!string.IsNullOrEmpty(invoice.Business?.Email))
                                    c.Item().Text(invoice.Business.Email).FontSize(8);
                            });
                        });

                        // Buyer (right)
                        row.RelativeItem().AlignRight().Column(buyer =>
                        {
                            buyer.Item().Text("Rechnungsempfänger (Käufer):").Bold().FontSize(9);
                            buyer.Item().PaddingTop(5).Column(c =>
                            {
                                c.Item().Text(invoice.Client.CompanyName).FontSize(10).Bold();
                                if (!string.IsNullOrEmpty(invoice.Client.ContactPerson))
                                    c.Item().Text(invoice.Client.ContactPerson).FontSize(9);
                                if (!string.IsNullOrEmpty(invoice.Client.City))
                                    c.Item().Text($"{invoice.Client.PostalCode} {invoice.Client.City}").FontSize(9);
                                c.Item().Text(invoice.Client.Country).FontSize(9);
                                if (!string.IsNullOrEmpty(invoice.Client.VatNumber))
                                    c.Item().Text($"UID: {invoice.Client.VatNumber}").FontSize(9);
                            });
                        });
                    });

                    col.Item().PaddingTop(15).LineHorizontal(1);

                    // Invoice items table
                    if (invoice.Items.Any())
                    {
                        col.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4f);   // Description
                                columns.RelativeColumn(1f);   // Qty
                                columns.RelativeColumn(1.5f); // Unit Price
                                columns.RelativeColumn(1.5f); // Amount
                            });

                            t.Header(header =>
                            {
                                header.Cell().Background("#333333").Padding(8).Text("Leistung/Ware").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Menge").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Einzelpreis").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Betrag").Bold().FontColor("white");
                            });

                            foreach (var item in invoice.Items)
                            {
                                t.Cell().Padding(5).Text(item.Description ?? "").FontSize(9);
                                t.Cell().Padding(5).AlignRight().Text(item.Quantity.ToString("N2")).FontSize(9);
                                t.Cell().Padding(5).AlignRight().Text($"€ {item.UnitPrice:N2}").FontSize(9);
                                t.Cell().Padding(5).AlignRight().Text($"€ {(item.Quantity * item.UnitPrice):N2}").FontSize(9);
                            }
                        });
                    }
                    else
                    {
                        // Fallback: show SubTotal as single line
                        col.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            t.Header(header =>
                            {
                                header.Cell().Background("#333333").Padding(8).Text("Leistung/Ware").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Menge").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Einzelpreis").Bold().FontColor("white");
                                header.Cell().Background("#333333").Padding(8).AlignRight().Text("Betrag").Bold().FontColor("white");
                            });

                            t.Cell().Padding(5).Text("(Siehe Details)").FontSize(9);
                            t.Cell().Padding(5).AlignRight().Text("1").FontSize(9);
                            t.Cell().Padding(5).AlignRight().Text($"€ {invoice.SubTotal:N2}").FontSize(9);
                            t.Cell().Padding(5).AlignRight().Text($"€ {invoice.SubTotal:N2}").FontSize(9);
                        });
                    }

                    // Summary section with tax information
                    col.Item().PaddingTop(15).Column(summary =>
                    {
                        summary.Item().Row(row =>
                        {
                            row.RelativeItem();  // Spacer

                            row.RelativeItem().Column(c =>
                            {
                                // Subtotal
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Summe netto:").AlignRight();
                                    r.ConstantItem(80).Text($"€ {invoice.SubTotal:N2}").AlignRight().Bold();
                                });

                                // Tax information based on invoice type
                                c.Item().PaddingTop(5).Row(r =>
                                {
                                    var taxLabel = GetTaxLabel(invoice);
                                    r.RelativeItem().Text(taxLabel).AlignRight();
                                    r.ConstantItem(80).Text($"€ {invoice.TaxAmount:N2}").AlignRight().Bold();
                                });

                                // Special remarks for tax cases
                                if (invoice.IsReverseCharge)
                                {
                                    c.Item().PaddingTop(5).Text("* Reverse Charge (Umkehrung der Steuerschuld)")
                                        .FontSize(8).Italic().FontColor("CC0000");
                                }
                                if (invoice.IsSmallBusinessExemption)
                                {
                                    c.Item().PaddingTop(5).Text("* Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG")
                                        .FontSize(8).Italic().FontColor("CC0000");
                                }
                                if (invoice.IsTaxFreeExport)
                                {
                                    c.Item().PaddingTop(5).Text("* Ausfuhrlieferung - steuerfrei")
                                        .FontSize(8).Italic().FontColor("CC0000");
                                }
                                if (invoice.IsIntraEUSale)
                                {
                                    c.Item().PaddingTop(5).Text("* Innergemeinschaftliche Lieferung")
                                        .FontSize(8).Italic().FontColor("CC0000");
                                }

                                // Total
                                c.Item().PaddingTop(10).LineHorizontal(1);
                                c.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Gesamtbetrag EUR:").AlignRight().Bold().FontSize(11);
                                    r.ConstantItem(80).Text($"€ {invoice.TotalAmount:N2}").AlignRight().Bold().FontSize(12);
                                });
                            });
                        });
                    });

                    // Payment and legal info
                    col.Item().PaddingTop(20).LineHorizontal(1);
                    col.Item().PaddingTop(10).Column(footer =>
                    {
                        footer.Item().Text("Zahlungsbedingungen:").Bold().FontSize(9);
                        footer.Item().Text($"Zahlbar bis: {invoice.DueDate:dd.MM.yyyy}").FontSize(9);
                        if (invoice.PaymentMethod != null)
                            footer.Item().Text($"Zahlungsart: {invoice.PaymentMethod.Name}").FontSize(9);

                        footer.Item().PaddingTop(10).Text("Rechtliche Hinweise:").Bold().FontSize(9);
                        footer.Item().Text("Diese Rechnung wurde elektronisch erstellt und ist gültig ohne Unterschrift.").FontSize(8);
                        footer.Item().Text("Gemäß österreichischem Umsatzsteuergesetz (UStG).").FontSize(8);
                    });

                    col.Item().PaddingTop(20).AlignCenter().Text($"Generated by QIMy ERP - {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(7).Italic().FontColor("999999");
                });
            });
        }).GeneratePdf();
    }

    private string GetInvoiceTypeLabel(InvoiceType type) => type switch
    {
        InvoiceType.Domestic => "Inland - Inlandslieferung",
        InvoiceType.Export => "Exportrechnung - Ausfuhrlieferung",
        InvoiceType.IntraEUSale => "Innergemeinschaftliche Lieferung",
        InvoiceType.ReverseCharge => "Reverse Charge Rechnung",
        InvoiceType.SmallBusinessExemption => "Kleinunternehmer",
        InvoiceType.TriangularTransaction => "Dreiecksgeschäfte",
        _ => "Rechnung"
    };

    private string GetTaxLabel(Invoice invoice) => invoice.InvoiceType switch
    {
        InvoiceType.Domestic => $"USt {invoice.TaxAmount / invoice.SubTotal * 100:F0}%:",
        InvoiceType.Export => "USt (steuerfrei):",
        InvoiceType.IntraEUSale => "USt (Umkehrung):",
        InvoiceType.ReverseCharge => "USt (Umkehrung):",
        InvoiceType.SmallBusinessExemption => "USt (Kleinunternehmer):",
        _ => "USt:"
    };
}
