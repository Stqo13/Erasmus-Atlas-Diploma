namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminEditUserViewModel
{
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Bio { get; set; }

    public List<string> CurrentRoles { get; set; } = new();

    public List<string> AllRoles { get; set; } = new();

    public List<string> SelectedRoles { get; set; } = new();
}
