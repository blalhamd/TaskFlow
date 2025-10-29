namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } 
    }
}
