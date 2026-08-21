using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class ReminderTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Channel { get; set; } // Email, SMS, WhatsApp
    public bool IsActive { get; set; } = true;
}
