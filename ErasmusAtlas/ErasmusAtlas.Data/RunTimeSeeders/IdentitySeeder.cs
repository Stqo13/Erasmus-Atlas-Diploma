using ErasmusAtlas.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace ErasmusAtlas.Infrastructure.RunTimeSeeders;

public static class IdentitySeeder
{
    public static readonly string[] Roles = { "Admin", "Moderator", "VerifiedStudent", "User" };

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var res = await roleManager.CreateAsync(new IdentityRole(role));
                if (!res.Succeeded)
                    throw new Exception($"Failed to create role '{role}': " +
                        string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }
    }

    public static async Task<List<ErasmusUser>> SeedUsersAsync(UserManager<ErasmusUser> userManager)
    {
        const string demoPassword = "Demo123!";

        var users = new (string Email, string Username, string FirstName, string LastName, string Role)[]
        {
            ("admin@erasmusatlas.com", "admin", "Admin", "User", "Admin"),
            ("moderator@erasmusatlas.com", "moderator", "Moderator", "User", "Moderator"),

            ("anna@demo.com", "anna", "Anna", "Ivanova", "VerifiedStudent"),
            ("george@demo.com", "george", "George", "Petrov", "VerifiedStudent"),
            ("maria@demo.com", "maria", "Maria", "Nikolova", "VerifiedStudent"),

            ("stefan@demo.com", "stefan", "Stefan", "Dimitrov", "User"),
            ("elena@demo.com", "elena", "Elena", "Georgieva", "User"),
            ("nikolay@demo.com", "nikolay", "Nikolay", "Todorov", "User"),
            ("ivana@demo.com", "ivana", "Ivana", "Stoyanova", "User"),
            ("boris@demo.com", "boris", "Boris", "Kolev", "User"),
        };

        var resultUsers = new List<ErasmusUser>();

        foreach (var u in users)
        {
            var user = await userManager.FindByEmailAsync(u.Email);

            if (user == null)
            {
                user = new ErasmusUser
                {
                    Email = u.Email,
                    UserName = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                };

                var createRes = await userManager.CreateAsync(user, demoPassword);
                if (!createRes.Succeeded)
                    throw new Exception($"Failed creating {u.Email}: " +
                        string.Join(", ", createRes.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, u.Role))
            {
                var addRoleRes = await userManager.AddToRoleAsync(user, u.Role);
                if (!addRoleRes.Succeeded)
                    throw new Exception($"Failed adding role {u.Role} to {u.Email}: " +
                        string.Join(", ", addRoleRes.Errors.Select(e => e.Description)));
            }

            resultUsers.Add(user);
        }

        return resultUsers;
    }
}
