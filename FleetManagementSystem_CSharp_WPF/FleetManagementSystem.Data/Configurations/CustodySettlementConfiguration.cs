using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class CustodySettlementConfiguration : IEntityTypeConfiguration<CustodySettlement>
    {
        public void Configure(EntityTypeBuilder<CustodySettlement> builder)
        {
            // CAST keeps the comparison numeric on SQLite, where EF stores decimal as TEXT.
            builder.ToTable("CustodySettlements", table => table.HasCheckConstraint(
                "CK_CustodySettlements_PositiveAmount",
                "CAST(Amount AS NUMERIC) > 0"));

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Amount).HasPrecision(18, 2);
            builder.Property(s => s.Description).HasMaxLength(500);
            builder.Property(s => s.ReceiptFilePath).IsRequired().HasMaxLength(500);
            builder.Property(s => s.Notes).HasMaxLength(2000);

            builder.HasIndex(s => s.CustodyId);

            builder.HasOne(s => s.Custody)
                .WithMany(c => c.Settlements)
                .HasForeignKey(s => s.CustodyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
