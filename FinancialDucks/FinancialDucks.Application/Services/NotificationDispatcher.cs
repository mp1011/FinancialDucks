using MediatR;
using System.Collections.Concurrent;

namespace FinancialDucks.Application.Services
{
    public class NotificationDispatcher<T>
         where T : INotification
    {
        private ConcurrentBag<INotificationHandler<T>> _handlers = new ConcurrentBag<INotificationHandler<T>>();

        public async Task DispatchEvent(T notification, CancellationToken cancellationToken)
        {
            var tasks = _handlers
                .Select(h => h.Handle(notification, cancellationToken))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        public void Register(INotificationHandler<T> handler)
        {
            _handlers.Add(handler);
        }

        public void Unregister(INotificationHandler<T> handler)
        {
            if(handler != null)
                _handlers.TryTake(out handler);
        }
    }
}
