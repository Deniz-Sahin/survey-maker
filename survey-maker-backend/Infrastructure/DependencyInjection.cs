using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SurveyMaker.Infrastructure.Data;
using SurveyMaker.Infrastructure.Identity;

namespace SurveyMaker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Expect an environment or appsettings connection string named "DefaultConnection"
        var conn = configuration.GetConnectionString("DefaultConnection")
                   ?? configuration["ConnectionStrings:DefaultConnection"]
                   ?? "Host=host.docker.internal;Database=surveydb;Username=survey_user;Password=password";

        services.AddDbContext<SurveyDbContext>(options =>
            options.UseNpgsql(conn, npgsqlOptions =>
            {
                // optional tuning: set MigrationsAssembly if you separate migrations project
                npgsqlOptions.EnableRetryOnFailure();
            }));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SurveyDbContext>()
            .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"] ?? "0GIoP3PIXBAnHvvlkYXNQY8sMmfFSoT9LMU02YKruWk";
        var issuer = jwtSection["Issuer"] ?? "SurveyMaker";
        var audience = jwtSection["Audience"] ?? "SurveyMakerAudience";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
        });

        return services;
    }
}