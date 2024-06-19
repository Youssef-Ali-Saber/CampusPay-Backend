using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class JWTHandlerService(IUnitOfWork unitOfWork, IConfiguration configuration)
{
    public async Task<string> GenerateToken(User user)
    {
        var claims = await _getClaimsAsync(user);
        var secureKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt")["Key"]));
        var signInCredentials = new SigningCredentials(secureKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: configuration.GetSection("Jwt")["Issuer"],
            audience: configuration.GetSection("Jwt")["Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: signInCredentials
            );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }


    private async Task<List<Claim>> _getClaimsAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        var userRoles = await unitOfWork.UserRepository.GetRolesAsync(user);
        foreach (var role in userRoles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        var userClaims = await unitOfWork.UserRepository.GetClaimsAsync(user);
        claims.Union(userClaims);
        return claims;
    }
}
