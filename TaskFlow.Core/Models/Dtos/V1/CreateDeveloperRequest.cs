using Microsoft.AspNetCore.Http;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Core.Models.Dtos.V1
{
    public class CreateDeveloperRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int Age { get; set; }
        public IFormFile? ImagePath { get; set; }
        public string JobTitle { get; set; } = null!;
        public int YearOfExperience { get; set; }
        public JobLevel JobLevel { get; set; }
    }
}
