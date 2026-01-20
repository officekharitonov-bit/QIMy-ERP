using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.Email)
            .HasMaxLength(100);
            
        builder.Property(c => c.Phone)
            .HasMaxLength(50);
            
        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Client)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
