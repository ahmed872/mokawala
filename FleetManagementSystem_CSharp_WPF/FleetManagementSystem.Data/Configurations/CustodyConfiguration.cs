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
            builder.Property(c => c.CustodyNumber).HasMaxLength(50);
            builder.Property(c => c.Status).HasMaxLength(30);
            builder.Property(c => c.Notes).HasMaxLength(2000);
            builder.Property(c => c.DocumentUrl).HasMaxLength(500);

            builder.HasOne(c => c.User)
                .WithMany(u => u.CustodyRecords)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
