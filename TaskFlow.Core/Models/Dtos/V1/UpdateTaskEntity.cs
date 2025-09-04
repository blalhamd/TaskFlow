using TaskFlow.Domain.Enums;

namespace TaskFlow.Core.Models.Dtos.V1
{
    public class UpdateTaskEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public TaskProgress Progress { get; set; }

        // Relationships
        public Guid AssignedToDeveloperId { get; set; }
    }
}
