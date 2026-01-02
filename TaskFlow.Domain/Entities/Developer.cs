using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities
{
    public class Developer : BaseEntity
    {
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

        private Developer() { } // EF Core will need it during reading from database
      
        private Developer(
            string fullName,
            int age, string? imagePath,
            string jobTitle, int yearOfExperience,
            JobLevel jobLevel, Guid userId)
        {
            FullName = fullName;
            Age = age;
            ImagePath = imagePath;
            JobTitle = jobTitle;
            YearOfExperience = yearOfExperience;
            JobLevel = jobLevel;
            UserId = userId;
        }

        // Static Factory method 
        public static ValueResult<Developer> Create(
            string fullName,
            int age, string? imagePath,
            string jobTitle, int yearOfExperience,
            JobLevel jobLevel, Guid userId)
        {
            var error = Validate(fullName, age, imagePath, jobTitle, yearOfExperience, jobLevel, userId);
            if(error != Error.None)
                return ValueResult<Developer>.Failure(error);

            return ValueResult<Developer>.Success(new Developer(fullName, age, imagePath, jobTitle, yearOfExperience, jobLevel, userId));
        } 

        // update method
        public Result Update(string fullName,
            int age, string? imagePath,
            string jobTitle, int yearOfExperience,
            JobLevel jobLevel, Guid userId)
        {
            var error = Validate(fullName, age, imagePath, jobTitle, yearOfExperience, jobLevel, userId);
            if (error != Error.None)
                return Result.Failure(error);

            FullName = fullName.Trim();
            Age = age;
            ImagePath = imagePath;
            JobTitle = jobTitle.Trim();
            YearOfExperience = yearOfExperience;
            JobLevel = jobLevel;
            UserId = userId;

            return Result.Success();
        }

        private static Error Validate(string fullName,
            int age, string? imagePath,
            string jobTitle, int yearOfExperience,
            JobLevel jobLevel, Guid userId)
        {
            if (string.IsNullOrEmpty(fullName))
                return DeveloperErrors.EmptyFullName;

            if (age < 18 || age > 80)
                return DeveloperErrors.InvalidAge;

            if (string.IsNullOrWhiteSpace(jobTitle))
                return DeveloperErrors.EmptyJobTitle;

            if (yearOfExperience < 0)
                return DeveloperErrors.InvalidExperience;

            if (userId == Guid.Empty)
                return DeveloperErrors.InvalidUserId;

            return Error.None;
        }
       
        public Result AssignToUser(Guid userId)
        {
            if (UserId == Guid.Empty)
                return Result.Failure(DeveloperErrors.InvalidUserId);

            UserId = userId;
            return Result.Success();
        }

        public Result AssignTask(TaskEntity task)
        {
            if (task == null) 
                return Result.Failure(new Error("Task.Null", "Task cannot be null", ErrorType.Validation));

            if (AssignedTasks.Any(t => t.Id == task.Id))
                return Result.Failure(DeveloperErrors.TaskAlreadyAssigned);

            AssignedTasks.Add(task);
            return Result.Success();
        }

        public Result RemoveTask(TaskEntity task)
        {
            if (task == null) return Result.Failure(new Error("Task.Null", "Task cannot be null", ErrorType.Validation));

            var removed = AssignedTasks.Remove(task);
            return removed ? Result.Success() : Result.Failure(new Error("Task.NotFound", "Task not found in developer list", ErrorType.NotFound));
        }
    }
}
