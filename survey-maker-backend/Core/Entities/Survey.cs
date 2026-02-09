using System.Collections.Generic;

namespace SurveyMaker.Core.Entities;

public class Survey
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}