using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public abstract record ChangeNotification<T>(T Data) : INotification
    { 
    }

    public abstract class ChangeNotificationHandler<T> : INotificationHandler<T>
        where T : INotification
    {
        private readonly NotificationDispatcher<T> _notificationDispatcher;

        public ChangeNotificationHandler(NotificationDispatcher<T> notificationDispatcher)
        {
            _notificationDispatcher = notificationDispatcher;
        }

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            await _notificationDispatcher.DispatchEvent(notification, cancellationToken);
        }
    }

    public record CategoryChangeNotification(ICategory Category) : ChangeNotification<ICategory>(Category)
    {
        public class Handler : ChangeNotificationHandler<CategoryChangeNotification>
        {
            public Handler(NotificationDispatcher<CategoryChangeNotification> notificationDispatcher) : base(notificationDispatcher) {            }
        }
    }
}
