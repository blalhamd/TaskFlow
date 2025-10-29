using TaskFlow.Shared.Common;

namespace TaskFlow.API.Extensions
{
    public static class CorsPolicy
    {
        public static IServiceCollection EnableCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()
                       .WithOrigins(configuration.GetSection(ApplicationConstants.Origins).Get<string[]>()!));
            });

            return services;
        }
    }
}
