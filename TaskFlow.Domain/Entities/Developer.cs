using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.Domain.Entities
{
    public class Developer : BaseEntity
    {
        public Developer(
            string fullName,
            int age, string? imagePath,
            string jobTitle, int yearOfExperience,
            JobLevel jobLevel, Guid userId)
        {
            SetFullName(fullName);
            SetAge(age);
            SetImagePath(imagePath);
            SetJobTitle(jobTitle);
            SetYearOfExperience(yearOfExperience);
            SetJobLevel(jobLevel); 
            AssignToUser(userId);
        }

        public string FullName { get; private set; } = null!;
        public int Age { get; private set; }
        public string? ImagePath { get; private set; }
        public string JobTitle { get; private set; } = null!;
        public int YearOfExperience { get; private set; }
        public JobLevel JobLevel { get; private set; }

        // Relation to User (identity/auth)
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public ICollection<TaskEntity> AssignedTasks { get; private set; } = new List<TaskEntity>();



        // ===== Business Methods (Encapsulation) =====
        public void SetFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new RequiredFieldMissingException("Full Name");

            FullName = fullName.Trim();
        }

        public void SetAge(int age)
        {
            if (age <= 0)
                throw new ValidationException("Age must be greater than zero");

            Age = age;
        }

        public void SetImagePath(string? imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && !imagePath.EndsWith(".jpg") && !imagePath.EndsWith(".png"))
                throw new ValidationException("Image path must be a valid JPG or PNG file");

            ImagePath = imagePath;
        }

        public void SetJobTitle(string jobTitle)
        {
            if (string.IsNullOrWhiteSpace(jobTitle))
                throw new RequiredFieldMissingException("Job Title");

            JobTitle = jobTitle.Trim();
        }

        public void SetYearOfExperience(int years)
        {
            if (years < 0)
                throw new ValidationException("Years of experience cannot be negative");

            YearOfExperience = years;
        }

        public void SetJobLevel(JobLevel jobLevel)
        {
            JobLevel = jobLevel;
        }

        public void AssignToUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("Invalid User Id");

            UserId = userId;
        }

        public void AssignTask(TaskEntity task)
        {
            if (task == null)
                throw new ValidationException("Task cannot be null");

            AssignedTasks.Add(task);
        }

        public void RemoveTask(TaskEntity task)
        {
            if (task == null)
                throw new ValidationException("Task cannot be null");

            AssignedTasks.Remove(task);
        }
    }

}
