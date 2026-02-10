using Microsoft.EntityFrameworkCore;
using SurveyMaker.Api.Application.Dtos;
using SurveyMaker.Infrastructure.Data;
using SurveyMaker.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SurveyMaker.Api.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly SurveyDbContext _db;

    public QuestionService(SurveyDbContext db)
    {
        _db = db;
    }

    public async Task<List<QuestionListItemDto>> ListAsync()
    {
        return await _db.Questions
            .Include(q => q.Options)
            .Include(q => q.Surveys)
            .Select(q => new QuestionListItemDto
            {
                Id = q.Id,
                Text = q.Text,
                IsMultipleChoice = q.IsMultipleChoice,
                Options = q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList(),
                Surveys = q.Surveys.Select(s => new SurveyListItemDto { Id = s.Id, Title = s.Title }).ToList()
            })
            .ToListAsync();
    }

    public async Task<QuestionDto?> GetAsync(int id)
    {
        var q = await _db.Questions
            .Include(x => x.Options)
            .Include(x => x.Surveys)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (q == null) return null;

        return new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            IsMultipleChoice = q.IsMultipleChoice,
            Options = q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList(),
            Surveys = q.Surveys.Select(s => new SurveyListItemDto { Id = s.Id, Title = s.Title }).ToList()
        };
    }

    public async Task<int> CreateAsync(CreateQuestionDto dto)
    {
        var options = dto.Options ?? new List<OptionDto>();

        ValidateOptions(dto.IsMultipleChoice, options);

        var question = new Question
        {
            Text = dto.Text,
            IsMultipleChoice = dto.IsMultipleChoice
        };

        foreach (var o in options)
        {
            question.Options.Add(new Option { Text = o.Text });
        }

        if (dto.SurveyIds != null && dto.SurveyIds.Any())
        {
            var surveys = await _db.Surveys.Where(s => dto.SurveyIds.Contains(s.Id)).ToListAsync();
            foreach (var s in surveys)
            {
                question.Surveys.Add(s);
            }
        }

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        return question.Id;
    }

    public async Task UpdateAsync(int id, UpdateQuestionDto dto)
    {
        var q = await _db.Questions
            .Include(x => x.Options)
            .Include(x => x.Surveys)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (q == null) throw new KeyNotFoundException();

        var options = dto.Options ?? new List<OptionDto>();
        ValidateOptions(dto.IsMultipleChoice, options);

        q.Text = dto.Text;
        q.IsMultipleChoice = dto.IsMultipleChoice;

        // Replace options
        _db.Options.RemoveRange(q.Options);
        q.Options.Clear();
        foreach (var o in options)
        {
            q.Options.Add(new Option { Text = o.Text });
        }

        // Update surveys
        q.Surveys.Clear();
        if (dto.SurveyIds != null && dto.SurveyIds.Any())
        {
            var surveys = await _db.Surveys.Where(s => dto.SurveyIds.Contains(s.Id)).ToListAsync();
            foreach (var s in surveys)
            {
                q.Surveys.Add(s);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) throw new KeyNotFoundException();

        _db.Questions.Remove(q);
        await _db.SaveChangesAsync();
    }

    private static void ValidateOptions(bool isMultipleChoice, List<OptionDto> options)
    {
        if (isMultipleChoice)
        {
            if (options.Count < 2 || options.Count > 4)
            {
                throw new ArgumentException("Multiple choice questions must have between 2 and 4 options.");
            }
        }
        else if (options.Count > 0)
        {
            throw new ArgumentException("Non-multiple-choice questions must not have options.");
        }
    }
}