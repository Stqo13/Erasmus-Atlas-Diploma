using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.ViewModels.AccountViewModels;

public class LoginViewModel
{
    [Required]
    public string UsernameOrEmail { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
