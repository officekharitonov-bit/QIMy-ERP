using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ii => ii.Quantity)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(ii => ii.Tax)
            .WithMany(t => t.InvoiceItems)
            .HasForeignKey(ii => ii.TaxId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
