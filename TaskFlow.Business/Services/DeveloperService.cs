using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskFlow.Business.Helper.Socket;
using TaskFlow.Core.IServices;
using TaskFlow.Core.IUnit;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;
using TaskFlow.Shared.Common;

namespace TaskFlow.Business.Services
{
    public class DeveloperService : IDeveloperService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IValidator<CreateDeveloperRequest> _validator;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DeveloperService> _logger;
        private readonly IHubContext<DeveloperHub> _developerHub;
        private readonly string _baseUrl;
        public DeveloperService(IUnitOfWorkAsync unitOfWork, IValidator<CreateDeveloperRequest> validator, UserManager<ApplicationUser> userManager, IImageService imageService, IConfiguration configuration, ILogger<DeveloperService> logger, IHubContext<DeveloperHub> developerHub)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _userManager = userManager;
            _imageService = imageService;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5001/images/";
            _developerHub = developerHub;
        }


        public async Task<ValueResult<PagesResult<DeveloperViewModel>>> GetAllDevelopers(int pageIndex, int pageSize)
        {
            _logger.LogInformation("Fetching developers page {PageIndex} with size {PageSize}", pageIndex, pageSize);

            pageIndex = Math.Max(pageIndex, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);
            var repo = _unitOfWork.DeveloperRepositoryAsync;

            var totalCount = await repo.CountAsync();
            var developers = await repo.GetAllAsync(predicate: null, orderBy: null, pageIndex, pageSize, includes: null!);

            if (!developers.Any())
                return ValueResult<PagesResult<DeveloperViewModel>>.Success(new PagesResult<DeveloperViewModel>([], pageIndex, pageSize, (int)totalCount));

            var developersVm = developers.Select(MapToDeveloperVM).ToList();

            _logger.LogInformation("Developers retrived successfully");
            return ValueResult<PagesResult<DeveloperViewModel>>.Success(new PagesResult<DeveloperViewModel>(developersVm, pageIndex, pageSize, (int)totalCount));
        }


        public async Task<Result> CreateDeveloper(CreateDeveloperRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new developer: {Email}", request.Email);

            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.First();
                return Result.Failure(new Error(error.ErrorCode, error.ErrorMessage, ErrorType.Validation));
            }

            var trimmedFullName = request.FullName.Trim();
            var trimmedJobTitle = request.JobTitle.Trim();

            var isExist = await _unitOfWork.DeveloperRepositoryAsync
                .IsExistAsync(d => d.FullName == trimmedFullName &&
                                   d.JobTitle == trimmedJobTitle &&
                                   d.YearOfExperience == request.YearOfExperience);

            if (isExist)
                return Result.Failure(DeveloperErrors.DeveloperAlreadyExist);

            var user = new ApplicationUser()
            {
                Email = request.Email,
                EmailConfirmed = true,
                UserName = request.Email.Split('@')[0],
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description, ErrorType.Validation));
            }

            string? newImagePath = null;
            try
            {
                if (request.ImagePath != null)
                {
                    newImagePath = await _imageService.UploadImageOnServer(request.ImagePath, false, null!, cancellationToken);
                    _logger.LogInformation("image created successfully on server");
                }

                var developerCreated = CreateNewDeveloper(request, newImagePath!, user.Id);
                if (!developerCreated.IsSuccess)
                {
                    if (newImagePath != null) _imageService.RemoveImage(newImagePath);
                    await _userManager.DeleteAsync(user);
                    return Result.Failure(developerCreated.Error);
                }

                await _unitOfWork.DeveloperRepositoryAsync.CreateAsync(developerCreated.Value, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _userManager.AddToRoleAsync(user, ApplicationConstants.Developer);
                _logger.LogInformation("created user is successfully and had developer role too.");

                // proadcasting to all online users
                await _developerHub.Clients.All.SendAsync("createdeveloper", MapToDeveloperVM(developerCreated.Value), cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(newImagePath))
                    _imageService.RemoveImage(newImagePath);

                await _userManager.DeleteAsync(user);
                throw;
            }
        }


        public async Task<ValueResult<DeveloperViewViewModel>> GetById(Guid id)
        {
            _logger.LogInformation("Get developer with {Id}", id);

            var developer = await _unitOfWork.DeveloperRepositoryAsync.GetByIdAsync(id, includes: x => x.AssignedTasks);
            if (developer is null)
                return ValueResult<DeveloperViewViewModel>.Failure(DeveloperErrors.NotFound);

            _logger.LogInformation("Developer with {Id} retrived successfully", id);

            return ValueResult<DeveloperViewViewModel>.Success(MapToDeveloperViewViewModel(developer));
        }


        public async Task<Result> UpdateDeveloper(UpdateDeveloperRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating developer with Id: {Id}", request.Id);

            var repo = _unitOfWork.DeveloperRepositoryAsync;
            var developer = await repo.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (developer is null)
                return Result.Failure(DeveloperErrors.NotFound);

            var oldPath = developer.ImagePath;
            string? newPath = null;
            if (request.ImagePath is not null)
            {
                newPath = await _imageService.UploadImageOnServer(request.ImagePath, false, oldPath!, cancellationToken);
            }

            var updateResult = developer.Update(request.FullName, request.Age, newPath ?? oldPath, request.JobTitle, request.YearOfExperience, request.JobLevel, developer.UserId);
            if (!updateResult.IsSuccess)
            {
                if (newPath != null) _imageService.RemoveImage(newPath);
                return Result.Failure(updateResult.Error);
            }

            try
            {
                await repo.UpdateAsync(developer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Remove the old image only after a successful update and if the image was changed
                if (!string.IsNullOrEmpty(oldPath) && !string.IsNullOrEmpty(newPath) && oldPath != newPath)
                {
                    _imageService.RemoveImage(oldPath);
                }

                await _developerHub.Clients.All.SendAsync("updateddeveloper", MapToDeveloperVM(developer), cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(newPath))
                    _imageService.RemoveImage(newPath);

                throw;
            }
        }


        public async Task<Result> DeleteDeveloper(Guid developerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting developer with Id: {Id}", developerId);

            // fetch developer from DB
            var repo = _unitOfWork.DeveloperRepositoryAsync;
            var developer = await repo.FirstOrDefaultAsync(x => x.Id == developerId);
            if (developer is null)
                return Result.Failure(DeveloperErrors.NotFound);

            // soft delete for developer
            await repo.DeleteAsync(developer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Optionally remove associated image
            if (!string.IsNullOrEmpty(developer.ImagePath))
            {
                _imageService.RemoveImage(developer.ImagePath);
            }

            // prodcasting to online clients
            await _developerHub.Clients.All.SendAsync("deletedeveloper", developer, cancellationToken);

            return Result.Success();
        }


        private DeveloperViewModel MapToDeveloperVM(Developer developer)
        {
            return new DeveloperViewModel
            {
                Id = developer.Id,
                FullName = developer.FullName,
                Age = developer.Age,
                JobLevel = developer.JobLevel,
                JobTitle = developer.JobTitle,
                UserId = developer.UserId,
                YearOfExperience = developer.YearOfExperience,
                ImagePath = string.IsNullOrEmpty(developer.ImagePath) ? _baseUrl : $"{_baseUrl}{developer.ImagePath}",
            };
        }


        private DeveloperViewViewModel MapToDeveloperViewViewModel(Developer developer)
        {
            return new DeveloperViewViewModel
            {
                Id = developer.Id,
                FullName = developer.FullName,
                Age = developer.Age,
                YearOfExperience = developer.YearOfExperience,
                ImagePath = string.IsNullOrEmpty(developer.ImagePath) ? _baseUrl : $"{_baseUrl}{developer.ImagePath}",
                JobLevel = developer.JobLevel,
                JobTitle = developer.JobTitle,
                UserId = developer.UserId,
                AssignedTasks = (developer.AssignedTasks.Count() > 0) ?
                developer.AssignedTasks.Select(MapToTaskEntityViewModel).ToList() : []
            };
        }


        private TaskEntityViewModel MapToTaskEntityViewModel(TaskEntity task)
        {
            return new TaskEntityViewModel
            {
                Id = task.Id,
                StartAt = task.StartAt,
                EndAt = task.EndAt,
                Content = task.Content,
                Document = string.IsNullOrEmpty(task.Document) ? null : $"{_baseUrl}{task.Document}",
                Progress = task.Progress,
                IsFinished = task.IsFinished,
                AssignedToDeveloperId = task.AssignedToDeveloperId,
            };
        }


        private ValueResult<Developer> CreateNewDeveloper(CreateDeveloperRequest request, string newImagePath, Guid userId)
           => Developer.Create(
                  request.FullName,
                  request.Age,
                  newImagePath,
                  request.JobTitle,
                  request.YearOfExperience,
                  request.JobLevel,
                  userId
                );

    }
}


