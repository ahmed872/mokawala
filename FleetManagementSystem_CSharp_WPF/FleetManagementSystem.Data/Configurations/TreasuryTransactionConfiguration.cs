using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagementSystem.Data.Configurations
{
    public class TreasuryTransactionConfiguration : IEntityTypeConfiguration<TreasuryTransaction>
    {
        public void Configure(EntityTypeBuilder<TreasuryTransaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TransactionType).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
            builder.Property(x => x.PaymentMethod).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(2000);
        }
    }
}
