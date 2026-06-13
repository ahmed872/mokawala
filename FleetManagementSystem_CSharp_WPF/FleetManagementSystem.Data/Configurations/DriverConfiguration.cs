using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.FullName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.NationalId).HasMaxLength(50);
            builder.Property(d => d.LicenseNumber).HasMaxLength(80);
            builder.Property(d => d.LicenseType).HasMaxLength(80);
            builder.Property(d => d.Governorate).HasMaxLength(100);
            builder.Property(d => d.FullAddress).HasMaxLength(500);
            builder.Property(d => d.TrafficUnit).HasMaxLength(150);
            builder.Property(d => d.WorkLocation).HasMaxLength(150);
        }
    }
}
