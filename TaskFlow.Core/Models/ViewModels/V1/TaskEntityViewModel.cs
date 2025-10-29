using TaskFlow.Domain.Enums;

namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class TaskEntityViewModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string? Content { get; set; }
        public string? Document { get; set; }
        public bool IsFinished { get; set; }
        public TaskProgress Progress { get; set; }

        // Relationships
        public Guid AssignedToDeveloperId { get; set; }
        public TimeSpan Duration => EndAt - StartAt;

    }
}
