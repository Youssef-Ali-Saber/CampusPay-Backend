using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.GenericRepository;

namespace Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext db,UserManager<User> userManager) : GenericRepository<User>(db), IUserRepository
{
    public async Task<IdentityResult> AddPasswordAsync(User user, string Password)
    {
        return await userManager.AddPasswordAsync(user, Password);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string type)
    {
        return await userManager.AddToRoleAsync(user, type);
    }

    public async Task<bool> CheckPasswordAsync(User user, string Password)
    {
        return await userManager.CheckPasswordAsync(user, Password);
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string Password)
    {
        return await userManager.CreateAsync(user, Password);
    }

    public async Task<IEnumerable<User>> GetAllInRoleAsync(string role)
    {
        return await userManager.GetUsersInRoleAsync(role);
    }

    public async Task<IList<Claim>> GetClaimsAsync(User user)
    {
        return await userManager.GetClaimsAsync(user);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsInRoleAsync(User user, string role)
    {
        return await userManager.IsInRoleAsync(user, role);
    }

    public async Task<IdentityResult> RemovePasswordAsync(User user)
    {
        return await userManager.RemovePasswordAsync(user);
    }
}
