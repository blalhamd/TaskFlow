using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskFlow.Core.Helpers;

namespace TaskFlow.API.Extensions
{
    public static class JwtAuthenticationConfig
    {
        public static IServiceCollection RegisterJwtAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtOptions = configuration.GetSection(JwtSetting.SectionName).Get<JwtSetting>();

            services.AddSingleton(JwtOptions!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.SaveToken = true;

                // here will validate parameters of Token.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "nameid",
                    ValidateIssuer = true,
                    ValidIssuer = JwtOptions!.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.Key))
                };
            });

            return services;
        }
    }
}
