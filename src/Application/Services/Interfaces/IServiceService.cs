using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces;

public interface IServiceService
{
    Task AddServiceAsync(ServiceDto service, string userId);
    Task EtitServiceAsync(int serviceId, ServiceDto serviceDto, string userId);
    Task DeleteServiceAsync(int serviceId, string userId);
    Task<IEnumerable<Service>?> GetServicesAsync(string userId);
    Task<Service?> GetServiceDetailsAsync(int serviceId);
    Task<string> PayServiceAsync(string userId, int serviceId);
}
