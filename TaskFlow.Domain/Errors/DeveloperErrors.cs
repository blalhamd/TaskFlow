using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Errors
{
    public static class DeveloperErrors
    {
        public static readonly Error EmptyFullName = new("Developer.EmptyFullName", "Full name cannot be empty", ErrorType.Validation);
        public static readonly Error InvalidAge = new("Developer.InvalidAge", "Age must be between 18 and 80", ErrorType.Validation);
        public static readonly Error EmptyJobTitle = new("Developer.JobTitleEmpty", "Job title cannot be empty", ErrorType.Validation);
        public static readonly Error InvalidExperience = new("Developer.InvalidExperience", "Years of experience cannot be negative", ErrorType.Validation);
        public static readonly Error InvalidUserId = new("Developer.InvalidUserId", "A valid User ID is required", ErrorType.Validation);
        public static readonly Error NotFound = new("Developer.NotFound", "Developer not found", ErrorType.NotFound);
        public static readonly Error TaskAlreadyAssigned = new("Developer.TaskAlreadyAssigned", "This task is already assigned to the developer", ErrorType.Conflict);
        public static readonly Error DeveloperAlreadyExist = new("Developer.DeveloperAlreadyExist", "This developer is already exist", ErrorType.Conflict);
    }
}
