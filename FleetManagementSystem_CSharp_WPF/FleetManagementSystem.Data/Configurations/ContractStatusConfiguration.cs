using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class ContractStatusConfiguration : IEntityTypeConfiguration<ContractStatus>
    {
        public void Configure(EntityTypeBuilder<ContractStatus> builder)
        {
            builder.HasKey(cs => cs.Id);
            builder.Property(cs => cs.Name).IsRequired().HasMaxLength(100);
        }
    }
}
