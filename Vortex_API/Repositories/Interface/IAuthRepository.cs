using Microsoft.AspNetCore.Identity;
using Vortex_API.Model.DTO;
namespace Vortex_API.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<string?> LoginAsync(LoginDTO dto);
        Task<IdentityResult> RegisterAsync(RegisterDTO dto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO dto);
        Task<ProfileDTO?> GetProfile(string userId);
    }
}
