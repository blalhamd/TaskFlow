using System.ComponentModel.DataAnnotations.Schema;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.Domain.Entities
{
    public class TaskEntity : BaseEntity
    {
        public TaskEntity(DateTimeOffset startAt, DateTimeOffset endAt, string? content, string? document, TaskProgress progress)
        {
            SetStartAt(startAt);
            SetEndAt(endAt);
            SetContent(content);
            SetDocument(document);
            SetProgress(progress);
        }

        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public string? Content { get; private set; } 
        public string? Document {  get; private set; }
        public bool IsFinished { get; private set; }
        public TaskProgress Progress { get; private set; }

        // Relationships
        public Guid AssignedToDeveloperId { get; private set; }
        public Developer AssignedToDeveloper { get; private set; } = null!;
        public List<Comment> Comments { get; set; } = [];

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

        public void SetContent(string? content)
        {
            if (string.IsNullOrEmpty(content))
                Content = null;
            else
            {
                if (content.Length > 1000)
                    throw new InvalidOperationException("Task Content can't skip 1000 characters");
                Content = content.Trim();
            }
        }

        public void SetDocument(string? doc)
        {
            if (string.IsNullOrEmpty(doc))
                Document = null;
            else
            {
                if (doc.Length > 2L * 1024 * 1024)
                    throw new InvalidOperationException("document size can't skip 2MB");
                Document = doc.Trim();
            }
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