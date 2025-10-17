using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
namespace Vortex_API.Repositories.Service
{
    public class UserManagementRepository:IUserManagementRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(
    string? filterOn = null,
    string? filterQuery = null,
    string? sortBy = null,
    bool isAscending = true,
    int pageNumber = 1,
    int pageSize = 10)
        {
            var users = _userManager.Users.AsQueryable();

            // --- Lọc (filter) ---
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                switch (filterOn.ToLower())
                {
                    case "username":
                        users = users.Where(u => u.UserName.Contains(filterQuery));
                        break;

                    case "email":
                        users = users.Where(u => u.Email.Contains(filterQuery));
                        break;

                    case "fullname":
                        users = users.Where(u => u.FullName.Contains(filterQuery));
                        break;

                    default:
                        break;
                }
            }

            // --- Sắp xếp (sort) ---
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "username":
                        users = isAscending
                            ? users.OrderBy(u => u.UserName)
                            : users.OrderByDescending(u => u.UserName);
                        break;

                    case "email":
                        users = isAscending
                            ? users.OrderBy(u => u.Email)
                            : users.OrderByDescending(u => u.Email);
                        break;

                    case "fullname":
                        users = isAscending
                            ? users.OrderBy(u => u.FullName)
                            : users.OrderByDescending(u => u.FullName);
                        break;

                    default:
                        break;
                }
            }

            // --- Phân trang ---
            var skipResults = (pageNumber - 1) * pageSize;
            users = users.Skip(skipResults).Take(pageSize);

            // --- Lấy dữ liệu ra kèm roles ---
            var userList = await users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles.ToList()
                });
            }

            return result;
        }


        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return false;

            var currentRoles = await _userManager.GetRolesAsync(user);

            // remove tất cả role hiện tại
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return false;

            // add role mới
            var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
            return addResult.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
