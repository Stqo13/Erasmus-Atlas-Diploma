namespace ErasmusAtlas.ViewModels.DashboardViewModels;

public class MonthlyActivityPointViewModel
{
    public string Month { get; set; } = null!;
    public int Count { get; set; }
    public double? GrowthRate { get; set; }
}
