using Microsoft.AspNetCore.Identity;
using SurveyMaker.Infrastructure.Identity;
using System.Threading.Tasks;

namespace SurveyMaker.Api.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> CreateUserAsync(string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var res = await _userManager.CreateAsync(user, password);
            return res.Succeeded;
        }
    }
}