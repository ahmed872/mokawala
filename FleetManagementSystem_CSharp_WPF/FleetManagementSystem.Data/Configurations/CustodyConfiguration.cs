using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class CustodyConfiguration : IEntityTypeConfiguration<Custody>
    {
        public void Configure(EntityTypeBuilder<Custody> builder)
        {
            builder.ToTable("Custody", table => table.HasCheckConstraint(
                "CK_Custody_ExactlyOneCustodian",
                "(DriverId IS NULL AND EmployeeId IS NOT NULL) OR (DriverId IS NOT NULL AND EmployeeId IS NULL)"));

            builder.HasKey(c => c.Id);
            builder.Property(c => c.CustodyNumber).IsRequired().HasMaxLength(80);
            builder.Property(c => c.Amount).HasPrecision(18, 2);
            builder.Property(c => c.Status).IsRequired().HasMaxLength(40);
            builder.Property(c => c.Notes).HasMaxLength(2000);
            builder.Property(c => c.DocumentUrl).HasMaxLength(500);

            builder.HasIndex(c => c.CustodyNumber).IsUnique();
            builder.HasIndex(c => c.DriverId);
            builder.HasIndex(c => c.EmployeeId);

            builder.HasOne(c => c.Driver)
                .WithMany(d => d.Custodies)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Employee)
                .WithMany(e => e.CustodyRecords)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.TreasuryTransaction)
                .WithMany(t => t.Custodies)
                .HasForeignKey(c => c.TreasuryTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
