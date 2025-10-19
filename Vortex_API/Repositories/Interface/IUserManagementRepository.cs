using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IUserManagementRepository
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(string? filterOn = null,
    string? filterQuery = null,
    string? sortBy = null,
    bool isAscending = true,
    int pageNumber = 1,
    int pageSize = 100);
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto);
        Task<bool> DeleteUserAsync(string userId);
    }
}
