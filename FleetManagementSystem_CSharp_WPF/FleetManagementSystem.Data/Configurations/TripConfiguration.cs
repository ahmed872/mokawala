using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.StartLocation).IsRequired().HasMaxLength(200);
            builder.Property(t => t.EndLocation).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Purpose).IsRequired().HasMaxLength(300);
            builder.Property(t => t.RequesterNameText).HasMaxLength(255);
            builder.Property(t => t.Status).HasMaxLength(50);
            builder.Property(t => t.Notes).HasMaxLength(1000);

            builder.HasIndex(t => t.VehicleId);
            builder.HasIndex(t => t.DriverId);
            builder.HasIndex(t => t.RequesterEmployeeId);
            builder.HasIndex(t => t.SupervisorEmployeeId);
            builder.HasIndex(t => t.StartDate);

            builder.HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RequesterEmployee)
                .WithMany(e => e.RequesterTrips)
                .HasForeignKey(t => t.RequesterEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SupervisorEmployee)
                .WithMany(e => e.SupervisorTrips)
                .HasForeignKey(t => t.SupervisorEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
