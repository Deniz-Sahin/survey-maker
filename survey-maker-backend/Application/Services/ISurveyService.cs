using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyMaker.Api.Application.Dtos;

namespace SurveyMaker.Api.Application.Services
{
    public interface ISurveyService
    {
        Task<IEnumerable<SurveyListItemDto>> ListAsync();
        Task<SurveyDto?> GetAsync(int id);
        Task<int> CreateAsync(CreateSurveyDto dto);
        Task UpdateAsync(int id, UpdateSurveyDto dto);
        Task DeleteAsync(int id);
        Task<int> FillAsync(int surveyId, string? userId, FillSurveyDto dto);
    }
}