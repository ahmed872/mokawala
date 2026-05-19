using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
}
