// Stryker disable all
using System.Reflection;

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

        private IEnumerable<PropertyInfo> GetAllPublicProperties<T>()
        {
            return GetAllPublicProperties(typeof(T));
        }

        private IEnumerable<PropertyInfo> GetAllPublicProperties(Type t)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            properties.AddRange(t.GetProperties());

            foreach(var implementedInterface in t.GetInterfaces())            
                properties.AddRange(GetAllPublicProperties(implementedInterface));
            
            return properties;
        }

        public void CopyAllProperties<TIn,TOut>(TIn source, TOut destination)
        {
            var outProperties = GetAllPublicProperties<TOut>();

            foreach (var inProperty in GetAllPublicProperties<TIn>())
            {
                if (!inProperty.CanRead)
                    continue;

                try
                {
                    var outProperty = outProperties.FirstOrDefault(p=>p.Name == inProperty.Name);
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
