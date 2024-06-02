using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeding;

public class UsersSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedUsersAsync()
    {
        var users = new List<User>
        {
            new User {FullName="Yousef Admin",SSN = "12345678912345", UserName = "yousef0admin", Email = "yousefalisaber0@gmail.com" },
            new User {FullName="Yousef Moderator1",SSN = "12345678954321", UserName = "yousef1moderator", Email = "yousefalisaber01@gmail.com"},
            new User {FullName="Yousef Moderator2",SSN = "12345678998765", UserName = "yousef2moderator", Email = "yousefalisaber11@gmail.com"}
        };

        foreach (var user in users)
        {
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                var result = await userManager.CreateAsync(user, "Asdfgh@123456");

                if (result.Succeeded)
                {
                    if (user.UserName == "yousef0admin")
                        await userManager.AddToRoleAsync(user, "Admin");
                    else if (user.UserName == "yousef1moderator" || user.UserName == "yousef2moderator")
                        await userManager.AddToRoleAsync(user, "Moderator");
                }
            }
        }
    }
}
