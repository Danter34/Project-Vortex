namespace Vortex.Models
{
    public class RegisterViewModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } = false;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string? Message { get; set; }
        public string? Role { get; set; }
    }
}
