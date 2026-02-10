using System.Collections.Generic;

namespace SurveyMaker.Api.Application.Dtos
{
    public class OptionDto
    {
        public int? Id { get; set; }
        public string Text { get; set; } = default!;
    }

    public class QuestionListItemDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = default!;
        public bool IsMultipleChoice { get; set; }
        public List<OptionDto> Options { get; set; } = new();
        public List<SurveyListItemDto> Surveys { get; set; } = new();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = default!;
        public bool IsMultipleChoice { get; set; }
        public List<OptionDto> Options { get; set; } = new();
        public List<SurveyListItemDto> Surveys { get; set; } = new();
    }

    public class CreateQuestionDto
    {
        public string Text { get; set; } = default!;
        public bool IsMultipleChoice { get; set; }
        public List<int>? SurveyIds { get; set; }
        public List<OptionDto>? Options { get; set; }
    }

    public class UpdateQuestionDto
    {
        public string Text { get; set; } = default!;
        public bool IsMultipleChoice { get; set; }
        public List<int>? SurveyIds { get; set; }
        public List<OptionDto>? Options { get; set; }
    }
}