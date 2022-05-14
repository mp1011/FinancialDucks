// Stryker disable all
using System.Reflection;

namespace FinancialDucks.Application.Services
{
    public interface IObjectMapper
    {
        void CopyAllProperties<T>(T source, T destination);
        TOut CopyIntoNew<TIn, TOut>(TIn source, Dictionary<Type, Type>? additionalMappings = null) where TOut : class, new();
        void CopyAllProperties<TIn, TOut>(TIn source, TOut destination, Dictionary<Type, Type>? additionalMappings = null);
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

        public TOut CopyIntoNew<TIn, TOut>(TIn source, Dictionary<Type,Type>? additionalMappings=null)
            where TOut:class,new()
        {
            var output = new TOut();
            CopyAllProperties<TIn,TOut>(source,output, additionalMappings);
            return output;
        }

        private object CopyIntoNew(Type inType, Type outType, object inObject, Dictionary<Type, Type>? additionalMappings = null)
        {
            var outObject = Activator.CreateInstance(outType);
            var copyMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(p => p.Name == nameof(CopyAllProperties) && p.GetGenericArguments().Length == 2);

            copyMethod = copyMethod.MakeGenericMethod(inType, outType);

            copyMethod.Invoke(this, new object[] { inObject, outObject, additionalMappings });

            return outObject;
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

        public void CopyAllProperties<TIn,TOut>(TIn source, TOut destination, Dictionary<Type, Type>? additionalMappings = null)
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

                    if(value != null && additionalMappings != null)
                    {
                        var mapType = GetMappingType(value.GetType(), additionalMappings);
                        if (mapType != null)
                            value = CopyIntoNew(value.GetType(), mapType, value, additionalMappings);
                    }

                    outProperty.SetValue(destination, value);
                }
                catch
                {
                    continue;
                }
            }
        }

        private Type? GetMappingType(Type inType, Dictionary<Type, Type>? additionalMappings = null)
        {
            if (additionalMappings == null)
                return null;

            var mapType = additionalMappings.GetValueOrDefault(inType);
            if (mapType != null)
                return mapType;


            var kv = additionalMappings.FirstOrDefault(k=> k.Key.IsAssignableFrom(inType));
            return kv.Value;
        }
    }
}
