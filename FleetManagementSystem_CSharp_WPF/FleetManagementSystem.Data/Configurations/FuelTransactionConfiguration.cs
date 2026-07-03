using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class FuelTransactionConfiguration : IEntityTypeConfiguration<FuelTransaction>
    {
        public void Configure(EntityTypeBuilder<FuelTransaction> builder)
        {
            builder.HasKey(ft => ft.Id);
            builder.Property(ft => ft.FuelType).HasMaxLength(50);
            builder.Property(ft => ft.FuelStation).HasMaxLength(255);
            builder.Property(ft => ft.Notes).HasMaxLength(2000);

            builder.HasOne(ft => ft.Vehicle)
                .WithMany(v => v.FuelTransactions)
                .HasForeignKey(ft => ft.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Trip)
                .WithMany(t => t.FuelTransactions)
                .HasForeignKey(ft => ft.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ft => ft.TreasuryTransaction)
                .WithMany(t => t.FuelTransactions)
                .HasForeignKey(ft => ft.TreasuryTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
