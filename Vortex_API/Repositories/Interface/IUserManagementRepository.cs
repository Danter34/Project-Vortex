using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IUserManagementRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto);
        Task<bool> DeleteUserAsync(string userId);
    }
}
