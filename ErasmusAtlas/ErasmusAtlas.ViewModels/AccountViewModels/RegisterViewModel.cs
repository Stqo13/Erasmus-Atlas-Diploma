using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.ViewModels.AccountViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = null!;

    [Required]
    [MinLength(3)]
    [MaxLength(32)]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can contain letters, numbers, dot, underscore and dash only.")]
    public string Username { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;
}
