
using TaskFlow.Core.Helpers;

namespace TaskFlow.API.Extensions
{
    public static class OptionsPatternExtension
    {
        public static IServiceCollection OptionsPatternConfig(this IServiceCollection services, IConfiguration configuration)
        {

            // Configure the JwtSetting options from the configuration

            services.Configure<JwtSetting>(configuration.GetSection(JwtSetting.SectionName));

            // Validate the configuration settings on start

            services.AddOptions<JwtSetting>(configuration[JwtSetting.SectionName])
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

            return services;
        }
    }
}
