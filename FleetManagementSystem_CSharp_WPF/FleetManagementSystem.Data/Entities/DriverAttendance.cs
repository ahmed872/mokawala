using System;

namespace FleetManagementSystem.Data.Entities
{
    public class DriverAttendance
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public DateTime WorkDate { get; set; }
        public string WorkLocation { get; set; } = string.Empty;
        public string Status { get; set; } = "Present";
        public string AbsenceReason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Driver? Driver { get; set; }
    }
}
