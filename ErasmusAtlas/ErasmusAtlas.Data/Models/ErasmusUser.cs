using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

using static ErasmusAtlas.Common.ApplicationConstraints.UserConstraints;

namespace ErasmusAtlas.Infrastructure.Models;

public class ErasmusUser : IdentityUser
{
    [MaxLength(UserFirstNameMaxLength)]
    public string? FirstName { get; set; }

    [MaxLength(UserLastNameMaxLength)]
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public ICollection<Post> Posts { get; set; }
        = new List<Post>();
    public ICollection<SavedPost> SavedPosts { get; set; } 
        = new List<SavedPost>();
    public ICollection<SavedProject> SavedProjects { get; set; } 
        = new List<SavedProject>();
}
