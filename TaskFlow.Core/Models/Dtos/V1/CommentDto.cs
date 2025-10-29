namespace TaskFlow.Core.Models.Dtos.V1
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
