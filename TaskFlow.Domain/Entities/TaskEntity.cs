using System.ComponentModel.DataAnnotations.Schema;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities
{
    public class TaskEntity : BaseEntity
    {
        // Private constructor which entity will create by Factory method
        private TaskEntity() { }

        private TaskEntity(
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            string? content,
            string? document,
            TaskProgress progress
            )
        {
            StartAt = startAt;
            EndAt = endAt;
            Content = NormalizeString(content);
            Document = NormalizeString(document);
            Progress = progress;
        }

        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public string? Content { get; private set; }
        public string? Document { get; private set; }
        public bool IsFinished { get; private set; }
        public TaskProgress Progress { get; private set; }

        // Relationships
        public Guid AssignedToDeveloperId { get; private set; }
        public Developer AssignedToDeveloper { get; private set; } = null!;
        public List<Comment> Comments { get; private set; } = [];

        [NotMapped]
        public TimeSpan Duration => EndAt - StartAt;

        // Factory Method
        public static ValueResult<TaskEntity> Create(
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            string? content,
            string? document
            )
        {
            // 1. Validate all inputs before creating the object
            var validationError = Validate(startAt, endAt, content, document);
            if (validationError != Error.None)
                return ValueResult<TaskEntity>.Failure(validationError);

            // 2. Create instance 
            var task = new TaskEntity(
                startAt,
                endAt,
                content,
                document,
                TaskProgress.NotStarted
                );

            return ValueResult<TaskEntity>.Success(task);
        }

        public Result Update(
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            string? content,
            string? document)
        {
            var validationError = Validate(startAt, endAt, content, document);
            if (validationError != Error.None)
                return Result.Failure(validationError);

            StartAt = startAt;
            EndAt = endAt;
            Content = NormalizeString(content);
            Document = NormalizeString(document);

            return Result.Success();
        }

        public Result MarkAsFinished()
        {
            if (IsFinished)
                return Result.Failure(TaskErrors.AlreadyFinished);

            IsFinished = true;
            Progress = TaskProgress.Completed;
            return Result.Success();
        }

        public Result Reopen()
        {
            if (!IsFinished)
                return Result.Failure(TaskErrors.NotFinished);

            IsFinished = false;
            Progress = TaskProgress.InProgress;
            return Result.Success();
        }

        public Result UpdateProgress(TaskProgress newProgress)
        {
            Progress = newProgress;

            if (newProgress == TaskProgress.Completed)
            {
                var finishResult = MarkAsFinished();
            }

            return Result.Success();
        }

        public Result AssignToDeveloper(Guid developerId)
        {
            if (developerId == Guid.Empty)
                return Result.Failure(TaskErrors.InvalidDeveloper);

            AssignedToDeveloperId = developerId;
            return Result.Success();
        }

        // Validation Logic
        private static Error Validate(
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            string? content,
            string? document)
        {
            if (startAt == default)
                return TaskErrors.EmptyStartDate;

            if (endAt == default)
                return TaskErrors.EmptyEndDate;

            if (startAt >= endAt)
                return TaskErrors.InvalidDateRange;

            if (content?.Length > 1000)
                return TaskErrors.ContentTooLong;

            if (document?.Length > 2 * 1024 * 1024)
                return TaskErrors.DocumentTooLarge;

            return Error.None;
        }

        private static string? NormalizeString(string? input)
        {
            return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
        }

    }
}