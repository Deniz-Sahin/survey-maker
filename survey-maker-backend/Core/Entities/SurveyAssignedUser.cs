using System;

namespace SurveyMaker.Core.Entities;

public class SurveyAssignedUser
{
    // Composite key (SurveyId, UserId) configured in DbContext
    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }

    // Identity user id (string)
    public string UserId { get; set; } = string.Empty;
}