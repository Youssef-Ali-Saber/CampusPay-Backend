using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.SeedingData;

public class RolesSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedRolesAsync()
    {
        var roles = new List<IdentityRole>
        {
            new IdentityRole { Name = "Student", NormalizedName = "STUDENT", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new IdentityRole { Name = "Donor", NormalizedName = "DONOR", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new IdentityRole { Name = "Moderator", NormalizedName = "MODERATOR", ConcurrencyStamp = Guid.NewGuid().ToString() }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }
}
