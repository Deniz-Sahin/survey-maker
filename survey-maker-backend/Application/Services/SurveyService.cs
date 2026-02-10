using Microsoft.EntityFrameworkCore;
using SurveyMaker.Api.Application.Dtos;
using SurveyMaker.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SurveyMaker.Core.Entities;
using System;

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
                    Description = s.Description,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                })
                .ToListAsync();
        }

        public async Task<SurveyDto?> GetAsync(int id)
        {
            var survey = await _db.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .Include(s => s.AssignedUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return null;

            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsActive = survey.IsActive,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                Questions = survey.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    IsMultipleChoice = q.IsMultipleChoice,
                    Options = q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList()
                }).ToList(),
                AssignedUserIds = survey.AssignedUsers.Select(a => a.UserId).ToList()
            };
        }

        public async Task<int> CreateAsync(CreateSurveyDto dto)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                throw new ArgumentException("StartDate must be before EndDate.");

            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                IsActive = dto.IsActive,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            // Questions: only accept existing question ids
            if (dto.QuestionIds != null && dto.QuestionIds.Any())
            {
                var distinctIds = dto.QuestionIds.Distinct().ToList();
                var existingQuestions = await _db.Questions
                    .Where(q => distinctIds.Contains(q.Id))
                    .ToListAsync();

                if (existingQuestions.Count != distinctIds.Count)
                {
                    // Provide clear validation message listing missing ids
                    var found = existingQuestions.Select(q => q.Id).ToHashSet();
                    var missing = distinctIds.Where(id => !found.Contains(id)).ToList();
                    throw new ArgumentException($"Some question ids are invalid: {string.Join(", ", missing)}");
                }

                foreach (var q in existingQuestions)
                {
                    survey.Questions.Add(q);
                }
            }

            // Assign users if provided: only add assignments for users that exist
            if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
            {
                var existingUserIds = await _db.Users
                    .Where(u => dto.AssignedUserIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var uid in existingUserIds)
                {
                    survey.AssignedUsers.Add(new SurveyAssignedUser { UserId = uid });
                }
            }

            _db.Surveys.Add(survey);
            await _db.SaveChangesAsync();
            return survey.Id;
        }

        public async Task UpdateAsync(int id, UpdateSurveyDto dto)
        {
            var survey = await _db.Surveys
                .Include(s => s.Questions)
                .Include(s => s.AssignedUsers)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) throw new KeyNotFoundException("Survey not found");

            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                throw new ArgumentException("StartDate must be before EndDate.");

            survey.Title = dto.Title;
            survey.Description = dto.Description;
            survey.IsActive = dto.IsActive;
            survey.StartDate = dto.StartDate;
            survey.EndDate = dto.EndDate;

            // Update Questions:
            // Clear existing relations (do not delete question entities).
            survey.Questions.Clear();

            if (dto.QuestionIds != null && dto.QuestionIds.Any())
            {
                var distinctIds = dto.QuestionIds.Distinct().ToList();
                var existingQuestions = await _db.Questions
                    .Where(q => distinctIds.Contains(q.Id))
                    .ToListAsync();

                if (existingQuestions.Count != distinctIds.Count)
                {
                    var found = existingQuestions.Select(q => q.Id).ToHashSet();
                    var missing = distinctIds.Where(i => !found.Contains(i)).ToList();
                    throw new ArgumentException($"Some question ids are invalid: {string.Join(", ", missing)}");
                }

                foreach (var q in existingQuestions)
                {
                    survey.Questions.Add(q);
                }
            }

            // Update assigned users: clear and add only existing users from provided list
            survey.AssignedUsers.Clear();
            if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
            {
                var existingUserIds = await _db.Users
                    .Where(u => dto.AssignedUserIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var uid in existingUserIds)
                {
                    survey.AssignedUsers.Add(new SurveyAssignedUser { UserId = uid });
                }
            }

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

        public async Task<SurveySubmissionsDto?> GetSubmissionsAsync(int surveyId)
        {
            var survey = await _db.Surveys
                .Include(s => s.AssignedUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null) return null;

            var assignedUserIds = survey.AssignedUsers.Select(a => a.UserId).ToList();

            // load assigned users' emails
            var assignedUsers = await _db.Users
                .Where(u => assignedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            // load responses and their answers
            var responses = await _db.SurveyResponses
                .Include(r => r.Answers)
                .Where(r => r.SurveyId == surveyId)
                .AsNoTracking()
                .ToListAsync();

            var responseDtos = responses.Select(r => new SurveyResponseDto
            {
                ResponseId = r.Id,
                UserId = r.UserId,
                UserEmail = assignedUsers.FirstOrDefault(u => u.Id == r.UserId)?.Email,
                SubmittedAt = r.SubmittedAt,
                Answers = r.Answers.Select(a => new AnswerDto
                {
                    QuestionId = a.QuestionId,
                    OptionId = a.OptionId,
                    TextAnswer = a.TextAnswer
                }).ToList()
            }).ToList();

            var respondedUserIds = responseDtos
                .Where(r => !string.IsNullOrEmpty(r.UserId))
                .Select(r => r.UserId!)
                .ToHashSet();

            var nonResponders = assignedUsers
                .Where(u => !respondedUserIds.Contains(u.Id))
                .Select(u => new UserListDto { Id = u.Id, Email = u.Email })
                .ToList();

            return new SurveySubmissionsDto
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                Responses = responseDtos,
                NonResponders = nonResponders
            };
        }

        public async Task<IEnumerable<AssignedSurveyListItemDto>> ListAssignedPendingAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<AssignedSurveyListItemDto>();

            // Surveys that have the user assigned and where the user has not yet submitted a response
            var query = _db.Surveys
                .AsNoTracking()
                .Where(s => s.AssignedUsers.Any(a => a.UserId == userId))
                .Where(s => !_db.SurveyResponses.Any(r => r.SurveyId == s.Id && r.UserId == userId))
                .Select(s => new AssignedSurveyListItemDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                });

            return await query.ToListAsync();
        }
    }
}