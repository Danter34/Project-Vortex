namespace Vortex_API.Model.DTO
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserRoleDto
    {
        public string UserId { get; set; } = null!;
        public string Role { get; set; } = null!; // "Admin" hoặc "User"
    }

    public class DeleteUserDto
    {
        public string UserId { get; set; } = null!;
    }
}
