namespace Vortex_API.Model.DTO
{
    public class ChangePasswordDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; } 
        public string ConfirmNewPassword { get; set; } 
    }
}
