using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Services;

public class PdfInvoiceGeneratorService
{
    public PdfInvoiceGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.MarginVertical(20);
                    page.MarginHorizontal(30);

                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("AUSGANGSRECHNUNG").FontSize(20).Bold();
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text($"Rechnung #{invoice.InvoiceNumber}").FontSize(12);
                            });
                        });

                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Rechnungsdatum:").SemiBold();
                                c.Item().Text(invoice.InvoiceDate.ToString("dd.MM.yyyy"));
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Fälligkeitsdatum:").SemiBold();
                                c.Item().Text(invoice.DueDate.ToString("dd.MM.yyyy"));
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Status:").SemiBold();
                                c.Item().Text(GetStatusText((int)invoice.Status));
                            });
                        });

                        col.Item().PaddingTop(15).Column(c =>
                        {
                            c.Item().Text("An:").SemiBold();
                            c.Item().Text(invoice.Client?.CompanyName ?? "Unbekannt");
                            if (!string.IsNullOrEmpty(invoice.Client?.Address))
                                c.Item().Text(invoice.Client.Address);
                            var cityline = $"{invoice.Client?.PostalCode ?? ""} {invoice.Client?.City ?? ""}".Trim();
                            if (!string.IsNullOrEmpty(cityline))
                                c.Item().Text(cityline);
                        });

                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(e => e.Background("2C3E50").Padding(5).Text("Beschreibung").FontColor("FFFFFF").SemiBold().FontSize(10));
                                header.Cell().Element(e => e.Background("2C3E50").Padding(5).AlignRight().Text("Qty").FontColor("FFFFFF").SemiBold().FontSize(10));
                                header.Cell().Element(e => e.Background("2C3E50").Padding(5).AlignRight().Text("Price").FontColor("FFFFFF").SemiBold().FontSize(10));
                                header.Cell().Element(e => e.Background("2C3E50").Padding(5).AlignRight().Text("Tax%").FontColor("FFFFFF").SemiBold().FontSize(10));
                                header.Cell().Element(e => e.Background("2C3E50").Padding(5).AlignRight().Text("Total").FontColor("FFFFFF").SemiBold().FontSize(10));
                            });

                            var items = invoice.Items?.Where(x => !x.IsDeleted).ToList() ?? new();
                            foreach (var item in items)
                            {
                                var taxRate = item.Tax?.TaxRate?.Rate ?? 0;
                                table.Cell().Padding(5).Text(item.Description).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{item.Quantity:F2}").FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{item.UnitPrice:F2}").FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{taxRate:F1}%").FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{item.TotalAmount:F2}").FontSize(9);
                            }
                        });

                        col.Item().PaddingTop(15).AlignRight().Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem(2).AlignRight().Text("Subtotal:").FontSize(10);
                                row.RelativeItem().AlignRight().Text($"{invoice.SubTotal:F2}").FontSize(10);
                            });

                            c.Item().Row(row =>
                            {
                                row.RelativeItem(2).AlignRight().Text("Tax:").FontSize(10);
                                row.RelativeItem().AlignRight().Text($"{invoice.TaxAmount:F2}").FontSize(10);
                            });

                            c.Item().Row(row =>
                            {
                                row.RelativeItem(2).AlignRight().Text("TOTAL:").Bold().FontSize(11);
                                row.RelativeItem().AlignRight().Text($"{invoice.TotalAmount:F2}").Bold().FontSize(11);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generated: {DateTime.Now:dd.MM.yyyy}").FontSize(8);
                });
            }).GeneratePdf();
    }

    private string GetStatusText(int status) => status switch
    {
        0 => "Draft",
        1 => "Sent",
        2 => "Paid",
        3 => "Partial",
        4 => "Overdue",
        5 => "Cancelled",
        _ => "Unknown"
    };
}

