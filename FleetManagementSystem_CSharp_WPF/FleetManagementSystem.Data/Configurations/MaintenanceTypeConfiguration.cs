using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class MaintenanceTypeConfiguration : IEntityTypeConfiguration<MaintenanceType>
    {
        public void Configure(EntityTypeBuilder<MaintenanceType> builder)
        {
            builder.HasKey(mt => mt.Id);
            builder.Property(mt => mt.Name).IsRequired().HasMaxLength(100);
        }
    }
}
