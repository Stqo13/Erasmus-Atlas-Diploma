namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
