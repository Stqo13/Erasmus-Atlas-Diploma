namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminUsersPageViewModel
{
    public IEnumerable<AdminUserListItemViewModel> Users { get; set; } 
        = new List<AdminUserListItemViewModel>();
}
