using TaskFlow.Domain.Enums;

namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class DeveloperViewModel 
    {
        public Guid Id { get; set; } 
        public string FullName { get; set; } = null!;
        public int Age { get; set; }
        public string? ImagePath { get; set; }
        public string JobTitle { get; set; } = null!;
        public int YearOfExperience { get; set; }
        public JobLevel JobLevel { get; set; }

        // Relation to User (identity/auth)
        public Guid UserId { get; set; }
    }
}
