using Domain.Entities;
using Domain.IRepositories.IGenericRepository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Domain.IRepositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<IEnumerable<User>> GetAllInRoleAsync(string role);
    Task<IdentityResult> CreateUserAsync(User user, string Password);
    Task<IdentityResult> AddToRoleAsync(User user, string type);
    Task<bool> CheckPasswordAsync(User user, string Password);
    Task<IList<string>> GetRolesAsync(User user);
    Task<IdentityResult> RemovePasswordAsync(User user);
    Task<IdentityResult> AddPasswordAsync(User user, string Password);
    Task<IList<Claim>> GetClaimsAsync(User user);
    Task<bool> IsInRoleAsync(User user, string role);
}
