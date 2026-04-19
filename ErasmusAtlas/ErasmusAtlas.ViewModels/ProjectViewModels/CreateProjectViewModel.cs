using ErasmusAtlas.ViewModels.SharedViewModels;
using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class CreateProjectViewModel
{
    [Required]
    [StringLength(160, MinimumLength = 5)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(4000, MinimumLength = 20)]
    public string Description { get; set; } = null!;

    [Required]
    public int CityId { get; set; }

    [Required]
    public int ProjectTypeId { get; set; }

    [Required]
    public string InstitutionName { get; set; } = null!;

    public List<int> TagIds { get; set; } = new();

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public IEnumerable<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();

    public IEnumerable<ProjectTypeLookupViewModel> ProjectTypes { get; set; } = new List<ProjectTypeLookupViewModel>();

    public IEnumerable<TagLookupViewModel> Tags { get; set; } = new List<TagLookupViewModel>();
}
