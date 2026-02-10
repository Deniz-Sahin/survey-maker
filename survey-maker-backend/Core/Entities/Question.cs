using System.Collections.Generic;

namespace SurveyMaker.Core.Entities;

public class Question
{
    public int Id { get; set; }

    public ICollection<Survey> Surveys { get; set; } = new List<Survey>();

    public string Text { get; set; } = string.Empty;
    public bool IsMultipleChoice { get; set; }

    public ICollection<Option> Options { get; set; } = new List<Option>();
}