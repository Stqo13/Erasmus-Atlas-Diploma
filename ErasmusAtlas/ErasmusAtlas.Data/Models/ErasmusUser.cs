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
}
