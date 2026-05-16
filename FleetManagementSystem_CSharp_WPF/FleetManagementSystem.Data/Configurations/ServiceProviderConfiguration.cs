using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProvider>
    {
        public void Configure(EntityTypeBuilder<ServiceProvider> builder)
        {
            builder.HasKey(sp => sp.Id);
            builder.Property(sp => sp.Name).IsRequired().HasMaxLength(255);
        }
    }
}
