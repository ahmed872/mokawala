using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Category).HasMaxLength(100);
            builder.Property(e => e.Vendor).HasMaxLength(255);
            builder.Property(e => e.Status).HasMaxLength(50);
            builder.Property(e => e.ReceiptUrl).HasMaxLength(500);
            builder.Property(e => e.Notes).HasMaxLength(2000);

            builder.HasOne(e => e.Vehicle)
                .WithMany(v => v.Expenses)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Custody)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CustodyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.TreasuryTransaction)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TreasuryTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
