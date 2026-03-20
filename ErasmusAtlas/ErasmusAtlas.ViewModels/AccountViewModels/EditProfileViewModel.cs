using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.ViewModels.AccountViewModels;

public class EditProfileViewModel
{
    [StringLength(80)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}
