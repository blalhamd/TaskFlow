using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Errors
{
    public static class TaskErrors
    {
        public static readonly Error EmptyStartDate = new("Task.EmptyStartDate", "Start date cannot be empty", ErrorType.Validation);
        public static readonly Error EmptyEndDate = new("Task.EmptyEndDate", "End date cannot be empty", ErrorType.Validation);
        public static readonly Error InvalidDateRange = new("Task.InvalidDateRange", "Start date must be before end date", ErrorType.Validation);
        public static readonly Error InvalidDeveloper = new("Task.InvalidDeveloper", "A valid developer ID is required", ErrorType.Validation);
        public static readonly Error ContentTooLong = new("Task.ContentTooLong", "Content cannot exceed 1000 characters", ErrorType.Validation);
        public static readonly Error DocumentTooLarge = new("Task.DocumentTooLarge", "Document exceeds size limit", ErrorType.Validation);
        public static readonly Error AlreadyFinished = new("Task.AlreadyFinished", "Task is already finished", ErrorType.Conflict);
        public static readonly Error NotFinished = new("Task.NotFinished", "Task is not finished yet", ErrorType.Conflict);
        public static readonly Error NotFound = new("Task.NotFound", "Task not found", ErrorType.NotFound);
    }
}
