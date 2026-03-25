using ErasmusAtlas.ViewModels.DashboardViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardPageViewModel> GetPageAsync();
    Task<DashboardOverviewViewModel> GetOverviewAsync(string? countryIso2 = null);
}
