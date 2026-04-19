using System.ComponentModel.DataAnnotations;

namespace RecipeShare_WebAPP.Models.Users
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
