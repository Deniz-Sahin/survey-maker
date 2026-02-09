using System.Collections.Generic;

namespace SurveyMaker.Api.Application.Dtos
{
    public class SurveyListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
    }

    public class OptionDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = default!;
    }

    public class QuestionDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = default!;
        public string? Type { get; set; } // e.g. "MultipleChoice", "Text"
        public List<OptionDto> Options { get; set; } = new();
    }

    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class CreateSurveyDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class UpdateSurveyDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
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
}