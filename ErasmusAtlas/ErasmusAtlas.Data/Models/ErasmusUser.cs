using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.Infrastructure.Models;

public class ErasmusUser : IdentityUser
{
    [MaxLength(80)]
    public string? FirstName { get; set; }

    [MaxLength(80)]
    public string? LastName { get; set; }
}
