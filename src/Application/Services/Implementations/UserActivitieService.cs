using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace Application.Services.Implementations;

public class UserActivitieService(IUnitOfWork unitOfWork, FilesUploader filesUploader) : IUserActivitieService
{

    public HistoryTransactionDto GetHistoryTransactions(string userId)
    {
        return new HistoryTransactionDto
        {
            transactions = unitOfWork.TransactionRepository.GetByFilter(t => t.UserId == userId).Include(s => s.Service).ToList(),
            deposits = unitOfWork.DepositionRepository.GetByFilter(d => d.UserId == userId).ToList(),
            transferFromMe = unitOfWork.TransformationRepository.GetByFilter(t => t.FromUserId == userId).ToList(),
            transferToMe = unitOfWork.TransformationRepository.GetByFilter(t => t.ToUserId == userId).ToList()
        };

    }
    public object? HistoryDonations(string userId)
    {

        return unitOfWork.DonationRepository.GetByFilter(m => m.UserId == userId, [i => i.User, i => i.SocialRequest])
                                .Select(s => new
                                {
                                    Service_Name = s.SocialRequest.Service.Name,
                                    amount = s.SocialRequest.Service.Cost,
                                    Student_Name = s.User.FullName.Split()[0],
                                    college = s.User.College_Name
                                });
    }
    public async Task AddFeedbackAsync(string massage, string userId)
    {
        var feedback = new Feedback
        {
            FeedBackMassage = massage,
            FeedBack_DateTime = DateTime.UtcNow,
            UserId = userId
        };
        await unitOfWork.FeedbackRepository.CreateAsync(feedback);
        await unitOfWork.SaveAsync();
    }

    public IEnumerable<object> GetFeedbacks()
    {
        return unitOfWork.FeedbackRepository.GetAll()
            .Include(u => u.User)
            .Select(s => new
            {
                feedBackDate = s.FeedBack_DateTime,
                feedBackMassage = s.FeedBackMassage,
                fullName = s.User.FullName,
                collegeName = s.User.College_Name
            });
    }


    public async Task<Dictionary<string, string>> UpdateProfileAsync(string userId, string? NewFullName, IFormFile? picture)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        if (!NewFullName.IsNullOrEmpty())
            user.FullName = NewFullName;
        Dictionary<string, string> errors = new();

        if (picture != null)
        {
            errors = filesUploader.ValidateFile(picture);
            if (errors.IsNullOrEmpty())
            {
                user.FilePath = await filesUploader.UploadPictureAsync(picture);
            }
        }
        unitOfWork.UserRepository.Update(user);
        await unitOfWork.SaveAsync();
        return errors;
    }
    public object? GetProfile(Expression<Func<User, bool>> filter)
    {
        var user = unitOfWork.UserRepository.GetByFilter(filter).FirstOrDefault();
        return new
        {
            picture = user.FilePath,
            email = user.Email,
            fullName = user.FullName,
            ssn = user.SSN,
            userId = user.Id
        };
    }

}
