using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.SeedingData
{
    public class UsersSeeder
    {
        private readonly UserManager<User> _userManager;

        public UsersSeeder(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new User { FullName = "Yousef Admin", SSN = "12345678912345", UserName = "yousef0admin", Email = "yousefalisaber0@gmail.com" },
                new User { FullName = "Yousef Moderator1", SSN = "12345678954321", UserName = "yousef1moderator", Email = "yousefalisaber01@gmail.com" },
                new User { FullName = "Yousef Moderator2", SSN = "12345678998765", UserName = "yousef2moderator", Email = "yousefalisaber11@gmail.com" }
            };

            foreach (var user in users)
            {
                if (await _userManager.FindByEmailAsync(user.Email) == null)
                {
                    var result = await _userManager.CreateAsync(user, "Asdfgh@123456");

                    if (result.Succeeded)
                    {
                        if (user.UserName == "yousef0admin")
                        {
                            await _userManager.AddToRoleAsync(user, "Admin");
                        }
                        else if (user.UserName == "yousef1moderator" || user.UserName == "yousef2moderator")
                        {
                            await _userManager.AddToRoleAsync(user, "Moderator");
                        }
                    }
                }
            }
        }
    }
}
