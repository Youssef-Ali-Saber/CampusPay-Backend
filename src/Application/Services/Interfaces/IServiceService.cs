using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces;

public interface IServiceService
{
    Task AddServiceAsync(ServiceDto service);
    Task EtitServiceAsync(int serviceId, ServiceDto serviceDto);
    Task DeleteServiceAsync(int serviceId);
    Task<IEnumerable<Service>?> GetServicesAsync(string userId);
    Task<Service?> GetServiceDetailsAsync(int serviceId);
    Task<string> PayServiceAsync(string userId, int serviceId);
}
