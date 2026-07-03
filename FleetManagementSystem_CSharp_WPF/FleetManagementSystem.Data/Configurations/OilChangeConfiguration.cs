using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagementSystem.Data.Configurations
{
    public class OilChangeConfiguration : IEntityTypeConfiguration<OilChange>
    {
        public void Configure(EntityTypeBuilder<OilChange> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OilType).HasMaxLength(100);
            builder.Property(x => x.ServiceItems).HasMaxLength(50);
            builder.Property(x => x.Status).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne(x => x.Vehicle)
                .WithMany(v => v.OilChanges)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
