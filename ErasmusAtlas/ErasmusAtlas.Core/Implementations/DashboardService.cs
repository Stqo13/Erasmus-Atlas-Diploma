using Microsoft.EntityFrameworkCore;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.ViewModels.DashboardViewModels;

namespace ErasmusAtlas.Core.Implementations
{
    public class DashboardService(
        IRepository<Post, Guid> postRepository,
        IRepository<City, int> cityRepository)
        : IDashboardService
    {
        public async Task<DashboardPageViewModel> GetPageAsync()
        {
            var countries = await cityRepository
                .GetAllAttached()
                .Where(c => !string.IsNullOrWhiteSpace(c.CountryIso2))
                .Select(c => new
                {
                    c.CountryIso2,
                    c.Name
                })
                .Distinct()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new DashboardPageViewModel
            {
                Countries = countries
                    .Select(c => new CountryOptionViewModel
                    {
                        Iso2 = c.CountryIso2!,
                        Name = string.IsNullOrWhiteSpace(c.Name) ? c.CountryIso2! : c.Name!
                    })
                    .ToList()
            };
        }

        public async Task<DashboardOverviewViewModel> GetOverviewAsync(string? countryIso2 = null)
        {
            IQueryable<Post> query = postRepository
                .GetAllAttached()
                .Include(p => p.City)
                .Include(p => p.PostTopics)
                .ThenInclude(pt => pt.Topic);

            if (!string.IsNullOrWhiteSpace(countryIso2))
            {
                countryIso2 = countryIso2.Trim().ToUpper();

                query = query.Where(p =>
                    p.City != null &&
                    p.City.CountryIso2 != null &&
                    p.City.CountryIso2.ToUpper() == countryIso2);
            }

            var posts = await query.ToListAsync();

            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var nextMonthStart = currentMonthStart.AddMonths(1);

            int totalPosts = posts.Count;

            int activeCountries = posts
                .Where(p => p.City != null && !string.IsNullOrWhiteSpace(p.City.CountryIso2))
                .Select(p => p.City!.CountryIso2!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            int activeCities = posts
                .Where(p => p.CityId.HasValue)
                .Select(p => p.CityId!.Value)
                .Distinct()
                .Count();

            int postsThisMonth = posts.Count(p =>
                p.CreatedAt >= currentMonthStart &&
                p.CreatedAt < nextMonthStart);

            int postsPreviousMonth = posts.Count(p =>
                p.CreatedAt >= previousMonthStart &&
                p.CreatedAt < currentMonthStart);

            double growthRate = 0;
            if (postsPreviousMonth > 0)
            {
                growthRate = ((double)(postsThisMonth - postsPreviousMonth) / postsPreviousMonth) * 100.0;
            }
            else if (postsThisMonth > 0)
            {
                growthRate = 100.0;
            }

            var topicCounts = posts
                .SelectMany(GetTopicNames)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .GroupBy(t => t)
                .Select(g => new
                {
                    Topic = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            int totalTopicMentions = topicCounts.Sum(t => t.Count);

            var topics = topicCounts
                .Select(t => new TopicStatViewModel
                {
                    Topic = t.Topic,
                    Count = t.Count,
                    Percentage = totalTopicMentions == 0
                        ? 0
                        : Math.Round((double)t.Count / totalTopicMentions * 100.0, 1)
                })
                .ToList();

            string topTopic = topics.FirstOrDefault()?.Topic ?? "N/A";

            var series = BuildMonthlySeries(posts, 12);
            double diversityScore = CalculateNormalizedShannonDiversity(topics);

            var insights = BuildInsights(
                totalPosts,
                postsThisMonth,
                growthRate,
                topics,
                diversityScore,
                activeCountries,
                activeCities);

            return new DashboardOverviewViewModel
            {
                TotalPosts = totalPosts,
                ActiveCountries = activeCountries,
                ActiveCities = activeCities,
                PostsThisMonth = postsThisMonth,
                GrowthRate = Math.Round(growthRate, 1),
                TopTopic = topTopic,
                TopicDiversityScore = Math.Round(diversityScore, 1),
                Topics = topics,
                Series = series,
                Insights = insights
            };
        }

        private static List<string> GetTopicNames(Post post)
        {
            return post.PostTopics?
                .Where(pt => pt.Topic != null && !string.IsNullOrWhiteSpace(pt.Topic.Name))
                .Select(pt => pt.Topic!.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();
        }

        private static List<MonthlyActivityPointViewModel> BuildMonthlySeries(List<Post> posts, int monthsBack)
        {
            var now = DateTime.UtcNow;
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-(monthsBack - 1));

            var grouped = posts
                .Where(p => p.CreatedAt >= startMonth)
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .ToDictionary(
                    g => $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                    g => g.Count());

            var result = new List<MonthlyActivityPointViewModel>();
            int? previousCount = null;

            for (int i = 0; i < monthsBack; i++)
            {
                var monthDate = startMonth.AddMonths(i);
                var monthKey = $"{monthDate.Year:D4}-{monthDate.Month:D2}";
                int count = grouped.TryGetValue(monthKey, out int value) ? value : 0;

                double? growthRate = null;

                if (previousCount.HasValue)
                {
                    if (previousCount.Value > 0)
                    {
                        growthRate = Math.Round(((double)(count - previousCount.Value) / previousCount.Value) * 100.0, 1);
                    }
                    else if (count > 0)
                    {
                        growthRate = 100.0;
                    }
                    else
                    {
                        growthRate = 0;
                    }
                }

                result.Add(new MonthlyActivityPointViewModel
                {
                    Month = monthKey,
                    Count = count,
                    GrowthRate = growthRate
                });

                previousCount = count;
            }

            return result;
        }

        private static double CalculateNormalizedShannonDiversity(List<TopicStatViewModel> topics)
        {
            if (topics.Count <= 1)
            {
                return 0;
            }

            int total = topics.Sum(t => t.Count);
            if (total == 0)
            {
                return 0;
            }

            double h = 0;

            foreach (var topic in topics)
            {
                double p = (double)topic.Count / total;
                if (p > 0)
                {
                    h -= p * Math.Log(p);
                }
            }

            double maxH = Math.Log(topics.Count);
            if (maxH == 0)
            {
                return 0;
            }

            return (h / maxH) * 100.0;
        }

        private static List<string> BuildInsights(
            int totalPosts,
            int postsThisMonth,
            double growthRate,
            List<TopicStatViewModel> topics,
            double diversityScore,
            int activeCountries,
            int activeCities)
        {
            var insights = new List<string>();

            if (totalPosts == 0)
            {
                insights.Add("No posts are available yet, so the dashboard will become more useful as more content is added.");
                return insights;
            }

            insights.Add($"The platform currently contains {totalPosts} posts across {activeCities} cities and {activeCountries} countries.");

            if (topics.Any())
            {
                var leadingTopic = topics.First();
                insights.Add($"{leadingTopic.Topic} is the most discussed topic at {leadingTopic.Percentage:F1}% of all topic mentions.");
            }

            if (growthRate > 0)
            {
                insights.Add($"Posting activity increased by {growthRate:F1}% compared to last month.");
            }
            else if (growthRate < 0)
            {
                insights.Add($"Posting activity decreased by {Math.Abs(growthRate):F1}% compared to last month.");
            }
            else
            {
                insights.Add("Posting activity is unchanged compared to last month.");
            }

            if (diversityScore >= 75)
            {
                insights.Add("Discussion is highly diverse, with topics distributed quite evenly.");
            }
            else if (diversityScore >= 45)
            {
                insights.Add("Discussion is moderately diverse, with a few stronger dominant themes.");
            }
            else
            {
                insights.Add("Discussion is concentrated around a small number of dominant topics.");
            }

            if (postsThisMonth > 0)
            {
                insights.Add($"{postsThisMonth} posts were created during the current month.");
            }

            return insights;
        }
    }
}
