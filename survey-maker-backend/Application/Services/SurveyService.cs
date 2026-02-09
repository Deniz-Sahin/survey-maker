using Microsoft.EntityFrameworkCore;
using SurveyMaker.Api.Application.Dtos;
using SurveyMaker.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SurveyMaker.Core.Entities;

namespace SurveyMaker.Api.Application.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly SurveyDbContext _db;

        public SurveyService(SurveyDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SurveyListItemDto>> ListAsync()
        {
            return await _db.Surveys
                .AsNoTracking()
                .Select(s => new SurveyListItemDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description
                })
                .ToListAsync();
        }

        public async Task<SurveyDto?> GetAsync(int id)
        {
            var survey = await _db.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return null;

            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                Questions = survey.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.IsMultipleChoice? "MultpilerChoice" : "Text",
                    Options = q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList()
                }).ToList()
            };
        }

        public async Task<int> CreateAsync(CreateSurveyDto dto)
        {
            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                Questions = dto.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    IsMultipleChoice = q.Type == "MultpilerChoice",
                    Options = q.Options.Select(o => new Option { Text = o.Text }).ToList()
                }).ToList()
            };

            _db.Surveys.Add(survey);
            await _db.SaveChangesAsync();
            return survey.Id;
        }

        public async Task UpdateAsync(int id, UpdateSurveyDto dto)
        {
            var survey = await _db.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) throw new KeyNotFoundException("Survey not found");

            survey.Title = dto.Title;
            survey.Description = dto.Description;

            // Simple replace strategy: remove existing questions and add new ones.
            _db.Questions.RemoveRange(survey.Questions);

            survey.Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text,
                IsMultipleChoice = q.Type == "MultpilerChoice",
                Options = q.Options.Select(o => new Option { Text = o.Text }).ToList()
            }).ToList();

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var survey = await _db.Surveys.FindAsync(id);
            if (survey == null) return;
            _db.Surveys.Remove(survey);
            await _db.SaveChangesAsync();
        }

        public async Task<int> FillAsync(int surveyId, string? userId, FillSurveyDto dto)
        {
            var surveyExists = await _db.Surveys.AnyAsync(s => s.Id == surveyId);
            if (!surveyExists) throw new KeyNotFoundException("Survey not found");

            var response = new SurveyResponse
            {
                SurveyId = surveyId,
                UserId = userId,
                Answers = dto.Answers.Select(a => new Answer
                {
                    QuestionId = a.QuestionId,
                    OptionId = a.OptionId,
                    TextAnswer = a.TextAnswer
                }).ToList()
            };

            _db.SurveyResponses.Add(response);
            await _db.SaveChangesAsync();

            return response.Id;
        }
    }
}