using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please choose a password.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Please choose a role.")]
        public string Role { get; set; } = "";
    }

    public class LoginResult
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}