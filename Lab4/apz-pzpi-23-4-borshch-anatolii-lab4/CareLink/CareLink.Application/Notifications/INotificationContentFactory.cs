namespace CareLink.Application.Notifications
{
    public interface INotificationContentFactory
    {
        string Build(long notificationTypeId, object context);
        string BuildGroup(object context);
    }
}