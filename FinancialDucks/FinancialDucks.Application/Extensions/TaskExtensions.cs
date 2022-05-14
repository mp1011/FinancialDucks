namespace FinancialDucks.Application.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> HandleError<T>(this Task<T> task, Func<Exception,Task> handleError)
        {
            try
            {
                var result = await task;
                return result;
            }
            catch(Exception e)
            {
                await handleError(e);
                e.Rethrow();
                throw; //line won't be reached, needed for compiler
            }
        }

        public static async Task HandleError(this Task task, Func<Exception, Task> handleError)
        {
            try
            {
                await task;                
            }
            catch (Exception e)
            {
                await handleError(e);
                e.Rethrow();
            }
        }
    }
}
