using iText.Kernel.Geom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementRepository _userManagementRepository;

        public UserManagementController(IUserManagementRepository userManagementRepository)
        {
            _userManagementRepository = userManagementRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
     string? filterOn,
     string? filterQuery,
     string? sortBy,
     bool isAscending = true,
     int pageNumber = 1,
     int pageSize = 10)
        {
            var users = await _userManagementRepository.GetAllUsersAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
            return Ok(users);
        }
        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto dto)
        {
            var success = await _userManagementRepository.UpdateUserRoleAsync(dto);
            if (!success) return BadRequest("Update role failed.");

            return Ok(new { message = "User role updated successfully" });
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var success = await _userManagementRepository.DeleteUserAsync(userId);
            if (!success) return BadRequest("Delete user failed.");

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
