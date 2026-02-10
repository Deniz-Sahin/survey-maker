using System.Collections.Generic;

namespace SurveyMaker.Core.Entities;

public class Survey
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<SurveyAssignedUser> AssignedUsers { get; set; } = new List<SurveyAssignedUser>();
}