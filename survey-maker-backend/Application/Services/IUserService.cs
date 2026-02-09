using System.Threading.Tasks;
using SurveyMaker.Api.Application.Dtos;

namespace SurveyMaker.Api.Application.Services
{
    public interface IUserService
    {
        Task<SurveyMaker.Infrastructure.Identity.ApplicationUser?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(string email, string password);
    }
}