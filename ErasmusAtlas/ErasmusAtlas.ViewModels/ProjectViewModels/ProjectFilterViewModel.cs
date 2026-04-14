namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class ProjectFilterViewModel
{
    public string? SearchTerm { get; set; }

    public int? CityId { get; set; }

    public int? ProjectTypeId { get; set; }

    public List<int> TagIds { get; set; } = new();
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}
