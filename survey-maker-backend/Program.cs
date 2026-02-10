using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SurveyMaker.Api.Application.Services;
using SurveyMaker.Infrastructure;
using SurveyMaker.Infrastructure.Data;
using SurveyMaker.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- CORS: read allowed origins from configuration (comma-separated) ---
var allowedOrigins = builder.Configuration.GetValue<string>("Cors:AllowedOrigins")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:64367" };

// debug: show resolved allowed origins
Console.WriteLine("CORS allowed origins: " + string.Join(", ", allowedOrigins));

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // TEMP for debugging only: if this fixes the issue, the problem is origin matching / redirect
    options.AddPolicy("PermissiveCorsForDebug", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Infrastructure (DbContext, Identity, JWT, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var config = builder.Configuration;

    var db = services.GetRequiredService<SurveyDbContext>();

    // Robust migration with retries to handle transient DB startup issues
    var maxAttempts = config.GetValue<int>("Db:MigrateMaxAttempts", 5);
    var baseDelayMs = config.GetValue<int>("Db:MigrateDelayMs", 2000);

    for (int attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database migrate failed (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}ms...", attempt, maxAttempts, baseDelayMs * attempt);
            await Task.Delay(baseDelayMs * attempt);
        }
    }

    // Conditional seeding controlled by configuration
    if (config.GetValue<bool>("SeedOnStartup", false))
    {
        var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

        async Task EnsureRole(string name)
        {
            if (!await roleMgr.RoleExistsAsync(name))
                await roleMgr.CreateAsync(new IdentityRole(name));
        }

        await EnsureRole("Admin");
        await EnsureRole("User");

        var adminEmail = config["AdminUser:Email"] ?? "admin@local";
        var adminPassword = config["AdminUser:Password"] ?? "Admin123!";

        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var res = await userMgr.CreateAsync(admin, adminPassword);
            if (res.Succeeded)
            {
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                logger.LogWarning("Failed to create initial admin user: {Errors}", string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS before authentication/authorization
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
