namespace ErasmusAtlas.ViewModels.DashboardViewModels;

public class DashboardOverviewViewModel
{
    public int TotalPosts { get; set; }
    public int ActiveCountries { get; set; }
    public int ActiveCities { get; set; }
    public int PostsThisMonth { get; set; }
    public double GrowthRate { get; set; }
    public string TopTopic { get; set; } = "N/A";
    public double TopicDiversityScore { get; set; }

    public List<TopicStatViewModel> Topics { get; set; } = new();
    public List<MonthlyActivityPointViewModel> Series { get; set; } = new();
    public List<string> Insights { get; set; } = new();
}
