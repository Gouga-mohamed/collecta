using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class ReminderRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DaysOverdue { get; set; }
    public string Action { get; set; } = string.Empty; // SendEmail, CreateTask, Escalate
    public Guid? ReminderTemplateId { get; set; }
    public ReminderTemplate? ReminderTemplate { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
}
