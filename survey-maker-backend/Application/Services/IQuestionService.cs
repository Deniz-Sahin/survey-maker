using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyMaker.Api.Application.Dtos;

namespace SurveyMaker.Api.Application.Services;

public interface IQuestionService
{
    Task<List<QuestionListItemDto>> ListAsync();
    Task<QuestionDto?> GetAsync(int id);
    Task<int> CreateAsync(CreateQuestionDto dto);
    Task UpdateAsync(int id, UpdateQuestionDto dto);
    Task DeleteAsync(int id);
}