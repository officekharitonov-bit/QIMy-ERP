using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();
            
        // Decimal precision for money
        builder.Property(i => i.SubTotal)
            .HasPrecision(18, 2);
            
        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2);
            
        builder.Property(i => i.PaidAmount)
            .HasPrecision(18, 2);
            
        // Relationships
        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
