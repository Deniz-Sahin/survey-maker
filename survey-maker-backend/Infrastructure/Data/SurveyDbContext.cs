using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SurveyMaker.Core.Entities;
using SurveyMaker.Infrastructure.Identity;

namespace SurveyMaker.Infrastructure.Data;

public class SurveyDbContext : IdentityDbContext<ApplicationUser>
{
    public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }

    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<Answer> Answers => Set<Answer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Question>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question!)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Survey>()
            .HasMany(s => s.Questions)
            .WithOne(q => q.Survey!)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SurveyResponse>()
            .HasMany(r => r.Answers)
            .WithOne(a => a.SurveyResponse!)
            .HasForeignKey(a => a.SurveyResponseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}