using System.Runtime.ExceptionServices;

namespace FinancialDucks.Application.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Rethrow(this Exception exception)
        {
            if (exception == null)
                return;

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
