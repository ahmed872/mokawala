using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.HasKey(v => v.Id);
            builder.Property(v => v.PlateNumber).IsRequired().HasMaxLength(50);
            builder.Property(v => v.ChassisNumber).HasMaxLength(100);
            builder.Property(v => v.EngineNumber).HasMaxLength(100);
            builder.Property(v => v.AccidentInsuranceDetails).HasMaxLength(500);
            builder.Property(v => v.SocialInsuranceDetails).HasMaxLength(500);
            builder.HasIndex(v => v.PlateNumber).IsUnique();
            builder.HasIndex(v => v.ChassisNumber);
            builder.HasIndex(v => v.EngineNumber);
        }
    }
}
