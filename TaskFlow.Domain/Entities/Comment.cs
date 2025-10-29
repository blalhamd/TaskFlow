namespace TaskFlow.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid TaskEntityId { get; set; }
        public TaskEntity TaskEntity { get; set; } = null!;
        public Guid DeveloperId { get; set; }
        public Developer Developer { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public bool IsDeleted { get; set; }
    }
}