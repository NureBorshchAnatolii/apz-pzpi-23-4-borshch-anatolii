using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using CareLink.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareLink.Persistence
{
    public static class PersistenceServicesDependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CareLinkDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ICognitiveExerciseTypeRepository, CognitiveExerciseTypeRepository>();
            services.AddScoped<ICognitiveResultRepository, CognitiveResultRepository>();
            services.AddScoped<IRelativeRepository, RelativeRepository>();
            services.AddScoped<ICognitiveExerciseRepository, CognitiveExerciseRepository>();
            services.AddScoped<IIoTDeviceRepository, IoTDeviceRepository>();
            services.AddScoped<IDeviceTypeRepository, DeviceTypeRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IIoTReadingRepository, IoTReadingRepository>();
            
            services.AddScoped<IRelationTypeRepository, RelationTypeRepository>();
            services.AddScoped<IDifficultyRepository, DifficultyRepository>();
            services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ISubscriptionPlanTypeRepository, SubscriptionPlanTypeRepository>();
            
            return services;
        }
    }
}