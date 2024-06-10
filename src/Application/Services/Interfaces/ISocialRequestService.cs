using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces;

public interface ISocialRequestService
{
    Task<IEnumerable<SocialRequest>?> GetAllAsync(string userId);
    SocialRequest? GetDetails(int socialRequestId);
    Task<string> DonateAsync(int SocialRequestId, string userId);
    Task CreateAsync(SocialRequestDto socialRequest, string Stu_Id);
    Task AcceptAsync(int id, bool status);
}
