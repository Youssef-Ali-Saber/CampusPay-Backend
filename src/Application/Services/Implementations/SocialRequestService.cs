using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;

namespace Application.Services.Implementations;

public class SocialRequestService(IUnitOfWork unitOfWork) : ISocialRequestService
{
    public async Task<IEnumerable<SocialRequest>?> GetAllAsync(string userId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Student"))
        {
            return unitOfWork.SocialRequestRepository.GetByFilter(s => s.StudentId == userId, [i => i.Service]);
        }
        else if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Donor"))
        {
            return unitOfWork.SocialRequestRepository.GetByFilter(s => s.Status == "Accept", [i => i.Service]);
        }
        else if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Moderator"))
        {
            return unitOfWork.SocialRequestRepository.GetByFilter(s => s.Status == "In Process", [i => i.Service]);
        }
        else
        {
            return null;
        }
    }
    public SocialRequest? GetDetails(int socialRequestId)
    {
        var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(m=>m.Id==socialRequestId , [n=>n.Service,n=>n.User]).FirstOrDefault();
        return socialRequest;
    }
    public async Task AcceptAsync(int id, bool status)
    {
        var socialRequest = await unitOfWork.SocialRequestRepository.GetByIdAsync(id);
        socialRequest.Status = status ? "Accept" : "Reject";
        unitOfWork.SocialRequestRepository.Update(socialRequest);
        await unitOfWork.SaveAsync();
    }

    public async Task CreateAsync(SocialRequestDto socialRequestDto, string Stu_Id)
    {
        var socialReqest = new SocialRequest
        {
            FatherName = socialRequestDto.FatherName,
            FatherIncome = socialRequestDto.FatherIncome,
            FatherIsDead = socialRequestDto.FatherIsDead,
            FatherJob = socialRequestDto.FatherJob,
            FatherState = socialRequestDto.FatherState,
            IsResidentInFamilyHome = socialRequestDto.IsResidentInFamilyHome,
            NumberOfFamilyMembers = socialRequestDto.NumberOfFamilyMembers,
            NumberOfFamilyMembersInEdu = socialRequestDto.NumberOfFamilyMembersInEdu,
            StudentId = Stu_Id,
            Status = "In Process",
            ServiceId = socialRequestDto.ServiceId
        };
        await unitOfWork.SocialRequestRepository.CreateAsync(socialReqest);
        await unitOfWork.SaveAsync();
    }

    public async Task<string> DonateAsync(int SocialRequestId, string userId)
    {
        var socialRequest = unitOfWork.SocialRequestRepository.GetByFilter(s => s.Id == SocialRequestId, [s => s.Service]).FirstOrDefault();
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                var donation = new Donation
                {
                    Date = DateTime.UtcNow,
                    SocialRequestId = socialRequest.Id,
                    UserId = userId                    
                };
                await unitOfWork.DonationRepository.CreateAsync(donation);
                await unitOfWork.SaveAsync();
                var donation1 = unitOfWork.AppWalletRepository.GetByFilter(a => a.Type == "Donations").FirstOrDefault();
                if (donation1 == null)
                {
                    var appWallet = new AppWallet
                    {
                        Balance = socialRequest.Service.Cost,
                        Type = "Donations"
                    };
                    await unitOfWork.AppWalletRepository.CreateAsync(appWallet);
                    await unitOfWork.SaveAsync();
                }
                else
                {
                    donation1.Balance += socialRequest.Service.Cost;
                }
                socialRequest.Status = "Donated";
                socialRequest.DonationId = donation.Id;
                unitOfWork.SocialRequestRepository.Update(socialRequest);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return "Donation done successful";
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return e.Message;
            }
        }

    }
}
