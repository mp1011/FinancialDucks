using FinancialDucks.Application.Models.AppModels;
using MediatR;

namespace FinancialDucks.Application.Services
{
    public class NotificationDispatcher<T>
         where T : INotification
    {
        private Dictionary<NotificationPriority, SynchronizedCollection<INotificationHandler<T>>> _handlers
            = new Dictionary<NotificationPriority, SynchronizedCollection<INotificationHandler<T>>>();


        public NotificationDispatcher()
        {
            foreach(NotificationPriority priority in Enum.GetValues<NotificationPriority>())
                _handlers[priority] = new SynchronizedCollection<INotificationHandler<T>>();
        }

        public async Task DispatchEvent(T notification, CancellationToken cancellationToken)
        {
            foreach(var priority in Enum.GetValues<NotificationPriority>()
                .OrderBy(p=>p))
            {
                await DispatchEvent(notification, priority, cancellationToken);
            }
        }

        private async Task DispatchEvent(T notification, NotificationPriority priority, CancellationToken cancellationToken)
        {
            var tasks = _handlers[priority]
                .Select(h => h.Handle(notification, cancellationToken))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        public void Register(INotificationHandler<T> handler, NotificationPriority priority)
        {
            _handlers[priority].Add(handler);
        }

        public void Unregister(INotificationHandler<T> handler)
        {
            if (handler == null)
                return;

            foreach(var handlerCollection in _handlers.Values)
                handlerCollection.Remove(handler);
        }
    }
}
