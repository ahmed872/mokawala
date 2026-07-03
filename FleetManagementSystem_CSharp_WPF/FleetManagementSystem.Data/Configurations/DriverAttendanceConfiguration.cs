using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetManagementSystem.Data.Configurations
{
    public class DriverAttendanceConfiguration : IEntityTypeConfiguration<DriverAttendance>
    {
        public void Configure(EntityTypeBuilder<DriverAttendance> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WorkDate).IsRequired();
            builder.Property(x => x.WorkLocation).HasMaxLength(255);
            builder.Property(x => x.Status).IsRequired().HasMaxLength(40);
            builder.Property(x => x.AbsenceReason).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.HasIndex(x => new { x.DriverId, x.WorkDate }).IsUnique();
            builder.HasOne(x => x.Driver)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
