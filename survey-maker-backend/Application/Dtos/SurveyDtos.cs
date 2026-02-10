using System;
using System.Collections.Generic;

namespace SurveyMaker.Api.Application.Dtos
{
    public class SurveyListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<QuestionDto> Questions { get; set; } = new();

        public List<string> AssignedUserIds { get; set; } = new();
    }

    public class CreateSurveyDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? QuestionIds { get; set; }
        public List<string>? AssignedUserIds { get; set; }
    }

    public class UpdateSurveyDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<int>? QuestionIds { get; set; }

        public List<string>? AssignedUserIds { get; set; }
    }

    public class FillAnswerDto
    {
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string? TextAnswer { get; set; }
    }

    public class FillSurveyDto
    {
        public List<FillAnswerDto> Answers { get; set; } = new();
    }

    public class FillResultDto
    {
        public int ResponseId { get; set; }
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string? TextAnswer { get; set; }
    }

    public class SurveyResponseDto
    {
        public int ResponseId { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class UserListDto
    {
        public string Id { get; set; } = default!;
        public string? Email { get; set; }
    }

    public class SurveySubmissionsDto
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = default!;
        public List<SurveyResponseDto> Responses { get; set; } = new();
        public List<UserListDto> NonResponders { get; set; } = new();
    }

    public class AssignedSurveyListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}