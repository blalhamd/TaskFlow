using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Business.Services;
using TaskFlow.Business.Services.Email;
using TaskFlow.Core.AutoMapper;
using TaskFlow.Core.IRepositories.Generic;
using TaskFlow.Core.IRepositories.Non_Generic;
using TaskFlow.Core.IServices;
using TaskFlow.Core.IServices.Email;
using TaskFlow.Core.IUnit;
using TaskFlow.Core.Models.Validators;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Infrastructure.Repositories.Generic;
using TaskFlow.Infrastructure.Repositories.Non_Generic;
using TaskFlow.Infrastructure.Unit;

namespace TaskFlow.DependencyInjection
{
    public static class Container
    {
        public static IServiceCollection RegisterConfiguration(this IServiceCollection services, IConfiguration configuration)
        {

            services.RegisterConnectionString(configuration)
                    .RegisterIdentity()
                    .RegisterRepositories()
                    .RegisterServices()
                    .RegisterUnitOfWork()
                    .RegisterAutoMapper()
                    .RegisterFluentValidation();
            

            return services;
        }

        private static IServiceCollection RegisterConnectionString(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration["ConnectionStrings:DefaultConnectionString"];
            services.AddDbContext<AppDbContext>(options=> options.UseSqlServer(connection));
            services.AddScoped<AppDbContext>();

            return services;
        }

        private static IServiceCollection RegisterIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password policy
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
              .AddEntityFrameworkStores<AppDbContext>() // store in DB
              .AddDefaultTokenProviders(); // for password reset, email confirm, etc.

            return services;
        }

        private static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IDeveloperService, DeveloperService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddScoped<IImageService, ImageService>();
           
            return services;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
            services.AddScoped(typeof(ITaskRepositoryAsync), typeof(TaskRepositoryAsync));

            return services;
        }

        private static IServiceCollection RegisterUnitOfWork(this IServiceCollection services)
        {

            services.AddScoped<IUnitOfWorkAsync, UnitOfWorkAsync>();

            return services;
        }

        private static IServiceCollection RegisterAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(x => { }, typeof(Mapping).Assembly);

            return services;
        }

        private static IServiceCollection RegisterFluentValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<CreateDeveloperRequestValidator>();
            
            return services;
        }

      
    }
}
