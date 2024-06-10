using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Services;

namespace Application.Services.Implementations;

public class ServiceService(IUnitOfWork unitOfWork, FilesUploader filesUploader) : IServiceService
{
    #region Manage Service 
    public async Task DeleteServiceAsync(int serviceId)
    {
        await unitOfWork.ServiceRepository.DeleteAsync<int>(serviceId);
        await unitOfWork.SaveAsync();
    }

    public async Task EtitServiceAsync(int serviceId, ServiceDto serviceDto)
    {
        var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
        service.Cost = serviceDto.Cost;
        service.Description = serviceDto.Description;
        service.Name = serviceDto.Name;
        service.Type = serviceDto.Type;
        service.SquadYear = serviceDto.SquadYear;
        service.CollegeName = serviceDto.CollegeName;
        if (serviceDto.Icon != null) service.FilePath = await filesUploader.UploadIconAsync(serviceDto.Icon);
        unitOfWork.ServiceRepository.Update(service);
        await unitOfWork.SaveAsync();
    }

    public async Task<Service?> GetServiceDetailsAsync(int serviceId)
    {
        var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
        return service;
    }
    public async Task AddServiceAsync(ServiceDto serviceDto)
    {
        var addService = new Service
        {
            Cost = serviceDto.Cost,
            Description = serviceDto.Description,
            Name = serviceDto.Name,
            Type = serviceDto.Type,
            SquadYear = serviceDto.SquadYear,
            CollegeName = serviceDto.CollegeName
        };
        var users = unitOfWork.UserRepository.GetAll();
        foreach (var user in users)
        {
            if (user.College_Name == addService.CollegeName && user.Squad_Year == addService.SquadYear)
            {
                if (addService.Users == null)
                    addService.Users = [];
                addService.Users.Add(user);
            }
        }
        if (serviceDto.Icon != null)
            addService.FilePath = await filesUploader.UploadIconAsync(serviceDto.Icon);
        await unitOfWork.ServiceRepository.CreateAsync(addService);
        await unitOfWork.SaveAsync();
    }
    #endregion

    public async Task<string> PayServiceAsync(string userId, int serviceId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        var service = await unitOfWork.ServiceRepository.GetByIdAsync(serviceId);
        if (user.Balance < service.Cost)
            return "balance not enough";
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                user.Balance -= service.Cost;
                var trans = new Transaction
                {
                    Date = DateTime.UtcNow,
                    ServiceId = service.Id,
                    UserId = user.Id,
                    Status = "Done"
                };
                var ServicePayment = unitOfWork.AppWalletRepository.GetByFilter(a => a.Type == "ServicePayments").FirstOrDefault();
                if (ServicePayment == null)
                {
                    var appWallet = new AppWallet
                    {
                        Balance = service.Cost,
                        Type = "ServicePayments"
                    };
                    await unitOfWork.AppWalletRepository.CreateAsync(appWallet);
                }
                else
                {
                    ServicePayment.Balance += service.Cost;
                }

                await unitOfWork.TransactionRepository.CreateAsync(trans);
                await unitOfWork.SaveAsync();
                transaction.Commit();
                return user.Balance.ToString();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    public async Task<IEnumerable<Service>?> GetServicesAsync(string userId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Student"))
        {
            return unitOfWork.ServiceRepository.GetByFilter(s => s.SquadYear == user.Squad_Year && s.CollegeName == user.College_Name);
        }
        else if (await unitOfWork.UserRepository.IsInRoleAsync(user, "Admin"))
        {
            return unitOfWork.ServiceRepository.GetAll();
        }
        else
        {
            return null;
        }
    }
}
