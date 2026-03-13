namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class ProjectFilterViewModel
{
    public string? SearchTerm { get; set; }

    public int? CityId { get; set; }

    public int? ProjectTypeId { get; set; }

    public List<int> TagIds { get; set; } = new();
}
