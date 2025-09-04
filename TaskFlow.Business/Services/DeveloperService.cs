using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskFlow.Core.IServices;
using TaskFlow.Core.IUnit;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Shared.Common;
using TaskFlow.Shared.Exceptions;

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
        private readonly string _baseUrl;
        public DeveloperService(IUnitOfWorkAsync unitOfWork, IValidator<CreateDeveloperRequest> validator, UserManager<ApplicationUser> userManager, IImageService imageService, IConfiguration configuration, ILogger<DeveloperService> logger)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _userManager = userManager;
            _imageService = imageService;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["BaseUrl"] ?? "default.png";
        }

        public async Task<PagesResult<DeveloperViewModel>> GetAllDevelopers(int pageIndex, int pageSize)
        {
            _logger.LogInformation("Fetching developers page {PageIndex} with size {PageSize}", pageIndex, pageSize);

            pageIndex = Math.Max(pageIndex, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);
            var repo = _unitOfWork.Repository<Developer>();

            var totalCount = await repo.CountAsync();
            var developers = await repo.GetAllAsync(predicate: null, orderBy: null, pageIndex, pageSize, includes: null!);

            if (!developers.Any())
                return new PagesResult<DeveloperViewModel>([], pageIndex, pageSize, (int)totalCount);

            var developersVm = developers.Select(developer => new DeveloperViewModel
            {
                Id = developer.Id,
                Age = developer.Age,
                FullName = developer.FullName,
                ImagePath = string.IsNullOrEmpty(developer.ImagePath) ? _baseUrl : $"{_baseUrl}/{developer.ImagePath}",
                JobLevel = developer.JobLevel,
                JobTitle = developer.JobTitle,
                UserId = developer.UserId,
                YearOfExperience = developer.YearOfExperience,
            })
                .ToList();

            _logger.LogInformation("Developers retrived successfully");
            return new PagesResult<DeveloperViewModel>(developersVm, pageIndex, pageSize, (int)totalCount);
        }

        public async Task<bool> CreateDeveloper(CreateDeveloperRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new developer: {Email}", request.Email);

            var validationResult = await _validator.ValidateAsync(request);

            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                _logger.LogWarning("Invalid data of request because {Errors}", errors);
                throw new BadRequestException(string.Join(",", errors));
            }

            var isExist = await _unitOfWork.Repository<Developer>()
                                .IsExistAsync(d=> d.FullName.ToLower() == request.FullName.Trim().ToLower() &&
                                                  d.JobTitle.ToLower() == request.JobTitle.Trim().ToLower() &&
                                                  d.YearOfExperience == request.YearOfExperience);

            if(isExist)
            {
                _logger.LogWarning("developer is already exist");
                throw new ItemAlreadyExistsException("developer is already exist");
            }

            var user = new ApplicationUser()
            {
                Email = request.Email,
                EmailConfirmed = true,
                UserName = request.Email.Split('@')[0],
                NormalizedUserName = request.Email.Split('@')[0].ToUpper(),
                NormalizedEmail = request.Email.ToUpper(),
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if(!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(x => x.Description).ToList();
                _logger.LogWarning("Can't create user because {Errors}", errors);
                throw new BadRequestException(string.Join(",", errors));
            }

            string? newImagePath = null;
            if (request.ImagePath != null)
            {
                newImagePath = await _imageService.UploadImageOnServer(request.ImagePath, false, null!, cancellationToken);
                _logger.LogInformation("image created successfully on server");
            }

            var developer = new Developer(
                request.FullName,
                request.Age,
                newImagePath,
                request.JobTitle,
                request.YearOfExperience,
                request.JobLevel,
                user.Id
                );

            await _unitOfWork.Repository<Developer>().CreateAsync(developer, cancellationToken);
            
            var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if(rowsAffected <= 0)
            {
                _logger.LogError("Developer could not be created due to database failure.");
                if (!string.IsNullOrEmpty(newImagePath))
                {
                   await _imageService.RemoveImage(newImagePath);
                }
                await _userManager.DeleteAsync(user);
                _logger.LogInformation("uploaded image removed from server");
                _logger.LogInformation("created user removed from database");
            }

            await _userManager.AddToRoleAsync(user, ApplicationConstants.Developer);
            _logger.LogInformation("created user is successfully and had developer role too.");

            return true;
        }

        public async Task<DeveloperViewViewModel> GetById(Guid id)
        {

            _logger.LogInformation("Get developer with {Id}", id);

            var developer = await _unitOfWork.Repository<Developer>().GetByIdAsync(id, includes: x => x.AssignedTasks);

            if (developer is null)
            {
                _logger.LogWarning("Developer with {Id} is not exist", id);
                throw new ItemNotFoundException("Developer is not exist");
            }

            var developerVM = new DeveloperViewViewModel
            {
                Id = id,
                FullName = developer.FullName,
                Age = developer.Age,
                YearOfExperience = developer.YearOfExperience,
                ImagePath = string.IsNullOrEmpty(developer.ImagePath) ? _baseUrl : $"{_baseUrl}/{developer.ImagePath}",
                JobLevel = developer.JobLevel,
                JobTitle = developer.JobTitle,
                UserId = developer.UserId,
                AssignedTasks = (developer.AssignedTasks.Count() > 0)? developer.AssignedTasks.Select(x => new TaskEntityViewModel
                {
                    Id = x.Id,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    Content = x.Content,
                    Progress = x.Progress,
                    IsFinished = x.IsFinished,
                    AssignedToDeveloperId = x.AssignedToDeveloperId,
                }).ToList() : []
            };

            _logger.LogInformation("Developer with {Id} retrived successfully", id);

            return developerVM;
        }

        public async Task<bool> UpdateDeveloper(UpdateDeveloperRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating developer with Id: {Id}", request.Id);

            var repo = _unitOfWork.Repository<Developer>();
            var developer = await repo.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (developer is null)
            {
                _logger.LogWarning("Developer with Id: {Id} does not exist", request.Id);
                throw new ItemNotFoundException("Developer does not exist");
            }

            developer.SetFullName(request.FullName);
            developer.SetJobLevel(request.JobLevel);
            developer.SetAge(request.Age);
            developer.SetJobTitle(request.JobTitle);
            developer.SetYearOfExperience(request.YearOfExperience);

            var oldPath = developer.ImagePath;
            string? newPath = null;
            if (request.ImagePath is not null)
            {
                newPath = await _imageService.UploadImageOnServer(request.ImagePath, false, oldPath!, cancellationToken);
                if (!string.IsNullOrEmpty(newPath))
                {
                    developer.SetImagePath(newPath);
                }
            }

            await repo.UpdateAsync(developer, cancellationToken);
            var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (rowsAffected <= 0)
            {
                if (!string.IsNullOrEmpty(newPath))
                {
                    await _imageService.RemoveImage(newPath);
                }
                return false;
            }

            // Remove the old image only after a successful update and if the image was changed
            if (!string.IsNullOrEmpty(oldPath) && !string.IsNullOrEmpty(newPath) && oldPath != newPath)
            {
                await _imageService.RemoveImage(oldPath);
            }

            return true;
        }

        public async Task<bool> DeleteDeveloper(Guid developerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting developer with Id: {Id}", developerId);

            var repo = _unitOfWork.Repository<Developer>();
            var developer = await repo.FirstOrDefaultAsync(x => x.Id == developerId);

            if (developer is null)
            {
                _logger.LogWarning("Developer with Id: {Id} does not exist", developerId);
                throw new ItemNotFoundException("Developer does not exist");
            }

            await repo.DeleteAsync(developer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Optionally remove associated image
            if (!string.IsNullOrEmpty(developer.ImagePath))
            {
                await _imageService.RemoveImage(developer.ImagePath);
            }

            return true;
        }
    }
}


