using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskFlow.Core.Helpers;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Entities.Identity;
using JsonClaimValueTypes = Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes;

namespace TaskFlow.Business.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtSetting _jwtSetting;

        public JwtProvider(IOptionsSnapshot<JwtSetting> jwtSetting)
        {
            _jwtSetting = jwtSetting.Value;
        }

        public JwtProviderResponse GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var descriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Expires = DateTime.Now.AddMinutes(_jwtSetting.LifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(nameof(roles), JsonSerializer.Serialize(roles),JsonClaimValueTypes.JsonArray),
                    new Claim(nameof(permissions), JsonSerializer.Serialize(permissions),JsonClaimValueTypes.JsonArray),
                })
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var createToken = tokenHandler.CreateToken(descriptor);
            var token = tokenHandler.WriteToken(createToken);

            return new JwtProviderResponse()
            {
                Token = token,
                ExpireIn = _jwtSetting.LifeTime
            };
        }
    }
}
