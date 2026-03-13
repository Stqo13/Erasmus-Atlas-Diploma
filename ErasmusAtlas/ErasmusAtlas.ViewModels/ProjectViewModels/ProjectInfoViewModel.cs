namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class ProjectInfoViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string DescriptionPreview { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Institution { get; set; } = null!;

    public string ProjectType { get; set; } = null!;

    public List<string> Tags { get; set; } = new();

    public DateTime CreatedOn { get; set; }
}
