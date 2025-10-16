namespace Vortex.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserRoleViewModel
    {
        public string UserId { get; set; } = "";
        public string Role { get; set; } = ""; // "Admin" hoặc "User"
    }
}
