namespace SurveyMaker.Core.Entities;

public class Answer
{
    public int Id { get; set; }

    public int SurveyResponseId { get; set; }
    public SurveyResponse? SurveyResponse { get; set; }

    public int QuestionId { get; set; }
    public int? OptionId { get; set; }
    public string? TextAnswer { get; set; }
}