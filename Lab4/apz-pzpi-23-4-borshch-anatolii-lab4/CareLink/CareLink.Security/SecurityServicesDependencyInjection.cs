using System.Security.Claims;
using System.Text;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Security;
using CareLink.Domain.Entities;
using CareLink.Security.Jwt;
using CareLink.Security.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CareLink.Security
{
    public static class SecurityServicesDependencyInjection
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordHasher<User>, MyPasswordHasher<User>>();
            
            services.AddHttpContextAccessor();

            services.AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        
                    RoleClaimType = ClaimTypes.Role
                };
            });
            services.AddAuthorization();
            
            return services;
        }
    }
}