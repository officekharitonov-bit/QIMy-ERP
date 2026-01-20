using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data.Configurations;

public class ExpenseInvoiceConfiguration : IEntityTypeConfiguration<ExpenseInvoice>
{
    public void Configure(EntityTypeBuilder<ExpenseInvoice> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.SubTotal)
            .HasPrecision(18, 2);
            
        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);
            
        builder.Property(e => e.PaidAmount)
            .HasPrecision(18, 2);
            
        builder.HasOne(e => e.Supplier)
            .WithMany(s => s.ExpenseInvoices)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Items)
            .WithOne(ei => ei.ExpenseInvoice)
            .HasForeignKey(ei => ei.ExpenseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
