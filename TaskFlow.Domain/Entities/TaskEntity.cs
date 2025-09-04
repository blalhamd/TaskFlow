using System.ComponentModel.DataAnnotations.Schema;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.Domain.Entities
{
    public class TaskEntity : BaseEntity
    {
        public TaskEntity(DateTimeOffset startAt, DateTimeOffset endAt, string content, TaskProgress progress)
        {
            SetStartAt(startAt);
            SetEndAt(endAt);
            SetContent(content);
            SetProgress(progress);
        }

        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public bool IsFinished { get; private set; }
        public TaskProgress Progress { get; private set; }

        // Relationships
        public Guid AssignedToDeveloperId { get; private set; }
        public Developer AssignedToDeveloper { get; private set; } = null!;

        [NotMapped]
        public TimeSpan Duration => EndAt - StartAt;

        // ===== Business Methods (Encapsulation) =====
        public void SetStartAt(DateTimeOffset startAt)
        {
            if (startAt == default)
                throw new ValidationException("Start date cannot be empty");

            if (EndAt != default && startAt >= EndAt)
                throw new ValidationException("Start date must be before end date");

            StartAt = startAt;
        }

        public void SetEndAt(DateTimeOffset endAt)
        {
            if (endAt == default)
                throw new ValidationException("End date cannot be empty");

            if (StartAt != default && endAt <= StartAt)
                throw new ValidationException("End date must be after start date");

            EndAt = endAt;
        }

        public void SetContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new RequiredFieldMissingException("Task Content");

            Content = content.Trim();
        }

        public void SetProgress(TaskProgress progress)
        {
            Progress = progress;

            // optional: auto-finish if progress == Completed
            if (progress == TaskProgress.Completed)
                MarkAsFinished();
        }

        public void MarkAsFinished()
        {
            if (IsFinished)
                throw new ValidationException("Task is already finished");

            IsFinished = true;
            Progress = TaskProgress.Completed;
        }

        public void Reopen()
        {
            if (!IsFinished)
                throw new ValidationException("Task is not finished yet");

            IsFinished = false;
            Progress = TaskProgress.InProgress; // or another default
        }

        public void AssignToDeveloper(Guid developerId)
        {
            if (developerId == Guid.Empty)
                throw new ValidationException("Invalid developer Id");

            AssignedToDeveloperId = developerId;
        }
    }

}