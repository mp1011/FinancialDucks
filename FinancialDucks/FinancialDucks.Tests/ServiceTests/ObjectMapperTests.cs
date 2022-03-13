#nullable disable
using FinancialDucks.Application.Services;
using System;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class ObjectMapperTests 
    {
        class DummyTypeA
        {
            public string PropertyA { get; set; }
            public int PropertyB { get; set; }
            public int ReadOnlyProperty { get; }

            public int WriteOnlyProperty
            {
                set
                {

                }
            }

            public string ThrowsError => throw new Exception("error");
        }

        class DummyTypeB
        {
            public string PropertyA { get; set; }
            public int PropertyB { get; set; }

            public int ReadOnlyProperty { get; }
            public int WriteOnlyProperty
            {
                set
                {

                }
            }

            public string ThrowsError => throw new Exception("error");
        }

        [Fact]
        public void CanConvertTypesWithIdenticalProperties()
        {
            var mapper = new ReflectionObjectMapper();

            var typeA = new DummyTypeA { PropertyA = "TEST STRING", PropertyB = 123 };

            var typeB = mapper.CopyIntoNew<DummyTypeA, DummyTypeB>(typeA);

            Assert.IsType<DummyTypeB>(typeB);
            Assert.Equal(typeA.PropertyA, typeB.PropertyA);
            Assert.Equal(typeA.PropertyB, typeB.PropertyB);
        }
    }
}
