using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities
{
    public class Comment
    {
        public int Id { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public Guid TaskEntityId { get; private set; }
        public TaskEntity TaskEntity { get; private set; } = null!;
        public Guid DeveloperId { get; private set; }
        public Developer Developer { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        // EF Core will needs it during reading from DB
        private Comment() { }

        // will need it in static factory method
        private Comment(string content, Guid taskEntityId, Guid developerId)
        {
            Content = content;
            TaskEntityId = taskEntityId;
            DeveloperId = developerId;
            CreatedAt = DateTimeOffset.Now;
            IsDeleted = false;
        }

        // static factory method to create comment object
        public static ValueResult<Comment> Create(string content, Guid taskEntityId, Guid developerId)
        {
            var error = Validate(content, taskEntityId, developerId);
            if (error != Error.None)
                return ValueResult<Comment>.Failure(error);

            var comment = new Comment(content, taskEntityId, developerId);

            return ValueResult<Comment>.Success(comment);
        }

        public Result Update(string content, Guid taskEntityId, Guid developerId)
        {
            var error = Validate(content, taskEntityId, developerId);
            if (error != Error.None)
                return Result.Failure(error);

            Content = content;
            TaskEntityId = taskEntityId;
            DeveloperId = developerId;

            return Result.Success();
        }

        private static Error Validate(string content, Guid taskEntityId, Guid developerId)
        {
            if (string.IsNullOrEmpty(content))
                return CommentErrors.EmptyContent;

            if(taskEntityId == Guid.Empty)
                return CommentErrors.EmptyTaskEntityId;

            if(developerId == Guid.Empty)
                return CommentErrors.EmptyDeveloperId;

            return Error.None;
        }
    }
}