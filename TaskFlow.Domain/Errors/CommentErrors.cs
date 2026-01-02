using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Errors
{
    public class CommentErrors
    {
        public static readonly Error EmptyContent = new("Comment.Errors.EmptyContent", "Content can't be null or empty", ErrorType.Validation);
        public static readonly Error EmptyTaskEntityId = new("Comment.Errors.EmptyTaskEntityId", "TaskEntity Id can't be null or empty", ErrorType.Validation);
        public static readonly Error EmptyDeveloperId = new("Comment.Errors.EmptyDeveloperId", "Developer Id can't be null or empty", ErrorType.Validation);

        public static readonly Error DatabaseError = new("Comment.Errors.DatabaseError", "Internal server error", ErrorType.InternalServerError);
    }
}
