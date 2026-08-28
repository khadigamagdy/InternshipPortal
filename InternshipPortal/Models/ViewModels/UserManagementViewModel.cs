namespace InternshipPortal.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsLocked { get; set; }

        public DateTime? LockoutEnd { get; set; }
    }
}