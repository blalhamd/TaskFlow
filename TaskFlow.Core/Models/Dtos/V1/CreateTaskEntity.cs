using Microsoft.AspNetCore.Http;

namespace TaskFlow.Core.Models.Dtos.V1
{
    public class CreateTaskEntity
    {
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string? Content { get; set; } 
        public IFormFile? Document { get; set; } 

        // Relationships
        public Guid AssignedToDeveloperId { get; set; }
    }
}
