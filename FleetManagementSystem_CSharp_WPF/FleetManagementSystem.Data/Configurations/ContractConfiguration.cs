using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ContractNumber).IsRequired().HasMaxLength(50);
        }
    }
}
