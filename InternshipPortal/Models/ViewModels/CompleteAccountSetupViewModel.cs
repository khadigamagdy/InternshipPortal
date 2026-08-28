using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class CompleteAccountSetupViewModel
    {
        [Required(ErrorMessage = "Please select an account type.")]
        public string Role { get; set; } = string.Empty;
    }
}