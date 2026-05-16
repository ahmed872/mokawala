using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data.Configurations
{
    public class CustodyConfiguration : IEntityTypeConfiguration<Custody>
    {
        public void Configure(EntityTypeBuilder<Custody> builder)
        {
            builder.HasKey(c => c.Id);
        }
    }
}
