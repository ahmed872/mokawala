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
            builder.Property(c => c.Amount).HasPrecision(18, 2);
            builder.Property(c => c.SettledAmount).HasPrecision(18, 2);
            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
