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

    public object GetHistoryTransactions(string userId)
    {
        return new
        {
            transactions = unitOfWork.TransactionRepository.GetByFilter(t => t.UserId == userId,[s => s.Service])
            .Select(s => new
            {
                date = DateOnly.FromDateTime(s.Date),
                time = $"{s.Date.Hour}:{s.Date.Minute}",
                serviceName = s.Service.Name,
                cost = s.Service.Cost,
                serviceType = s.Service.Type

            }).ToList(),
            deposits = unitOfWork.DepositionRepository.GetByFilter(d => d.UserId == userId)
            .Select(s => new
            {
                date = DateOnly.FromDateTime(s.Date),
                time = $"{s.Date.Hour}:{s.Date.Minute}",
                balance = s.Balance
            }).ToList(),
            transferFromMe = unitOfWork.TransformationRepository.GetByFilter(t => t.FromUserId == userId)
            .Select(s => new
            {
                balance = s.Balance,
                date = DateOnly.FromDateTime(s.Date),
                time = $"{s.Date.Hour}:{s.Date.Minute}",
                FromUserFullName = s.FromUser.FullName,
                ToUserFullName = unitOfWork.UserRepository.GetByIdAsync(s.ToUserId).Result.FullName


            }).ToList(),
            transferToMe = unitOfWork.TransformationRepository.GetByFilter(t => t.ToUserId == userId)
            .Select(s => new
            {
                balance = s.Balance,
                date = DateOnly.FromDateTime(s.Date),
                time = $"{s.Date.Hour}:{s.Date.Minute}",
                FromUserFullName = s.FromUser.FullName,
                ToUserFullName = unitOfWork.UserRepository.GetByIdAsync(s.ToUserId).Result.FullName
            }).ToList()
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
                                    college = s.User.College_Name,
                                    squadYear = s.User.Squad_Year,
                                    date = DateOnly.FromDateTime(s.Date),
                                    time = $"{s.Date.Hour}:{s.Date.Minute}"
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

    public decimal? TotalOfMoneyPayed(string userId)
    {
        var transactions = unitOfWork.TransactionRepository.GetByFilter(t => t.UserId == userId, [m => m.Service]);

        var totalOfMoneyPayed = transactions?.Sum(s => s.Service.Cost);

        var transferFromMe = unitOfWork.TransformationRepository.GetByFilter(t => t.FromUserId == userId);

        var total = totalOfMoneyPayed + transferFromMe.Sum(s => s.Balance);

        return total;
    }

    public decimal? TotalOfMoneyDeposited(string userId)
    {
        var deposits = unitOfWork.DepositionRepository.GetByFilter(d => d.UserId == userId);

        var transferToMe = unitOfWork.TransformationRepository.GetByFilter(t => t.ToUserId == userId);

        var totalOfMoneyDeposited = deposits.Sum(s => s.Balance);

        var total = totalOfMoneyDeposited + transferToMe.Sum(s => s.Balance);

        return total;

    }
}
