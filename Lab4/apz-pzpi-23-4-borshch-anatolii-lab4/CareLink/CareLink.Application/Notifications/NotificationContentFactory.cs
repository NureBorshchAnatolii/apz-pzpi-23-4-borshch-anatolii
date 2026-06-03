using CareLink.Application.Dtos.Messages;

namespace CareLink.Application.Notifications
{
    public class NotificationContentFactory : INotificationContentFactory
    {
        public string Build(long typeId, object context)
        {
            return typeId switch
            {
                1 => BuildSystem(context),
                2 => BuildHealthAlert(context),
                3 => BuildMedication(context),
                4 => BuildFallDetected(context),
                _ => "Unknown notification"
            };
        }

        private string BuildSystem(object context)
        {
            if (context is MessageCreateRequest r)
                return $"New message from user {r.SenderId}";

            return "System notification";
        }

        private string BuildHealthAlert(object context)
            => "Health indicators are out of normal range";

        private string BuildMedication(object context)
            => "Time to take your medication";

        private string BuildFallDetected(object context)
            => "Possible fall detected. Please check immediately.";
    
        public string BuildGroup(object context)
            => Guid.NewGuid().ToString();
    }
}