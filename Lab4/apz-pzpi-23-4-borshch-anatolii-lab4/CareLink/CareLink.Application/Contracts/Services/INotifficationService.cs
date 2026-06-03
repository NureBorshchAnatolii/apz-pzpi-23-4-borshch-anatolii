using CareLink.Application.Dtos.Notiffications;
using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Services
{
    public interface INotifficationService
    {
        Task<NotificationDto> GetUserNotiffication(long userId);
    }
}