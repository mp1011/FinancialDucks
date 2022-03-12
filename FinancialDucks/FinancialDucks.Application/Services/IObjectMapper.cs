namespace FinancialDucks.Application.Services
{
    public interface IObjectMapper
    {
        void CopyAllProperties<T>(T source, T destination);
        TOut CopyIntoNew<TIn, TOut>(TIn source) where TOut : class, new();
        void CopyAllProperties<TIn, TOut>(TIn source, TOut destination);
    }

    public class ReflectionObjectMapper : IObjectMapper
    {
        public void CopyAllProperties<T>(T source, T destination)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                try
                {
                    var value = property.GetValue(source);
                    property.SetValue(destination, value);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        public TOut CopyIntoNew<TIn, TOut>(TIn source)
            where TOut:class,new()
        {
            var output = new TOut();
            CopyAllProperties<TIn,TOut>(source,output);
            return output;
        }

        public void CopyAllProperties<TIn,TOut>(TIn source, TOut destination)
        {
            foreach (var inProperty in typeof(TIn).GetProperties())
            {
                if (!inProperty.CanRead)
                    continue;

                try
                {
                    var outProperty = typeof(TOut).GetProperty(inProperty.Name);
                    if (outProperty == null || !outProperty.CanWrite)
                        continue;

                    var value = inProperty.GetValue(source);
                    outProperty.SetValue(destination, value);
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
