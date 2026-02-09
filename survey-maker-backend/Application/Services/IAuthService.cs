using System.Threading.Tasks;

namespace SurveyMaker.Api.Application.Services
{
    public interface IAuthService
    {
        Task<string?> GenerateJwtAsync(string email, string password);
    }
}