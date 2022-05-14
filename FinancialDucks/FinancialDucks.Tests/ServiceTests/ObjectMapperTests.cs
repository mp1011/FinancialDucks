#nullable disable
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class ObjectMapperTests 
    {
        public interface IDummyInterfaceBase
        {
            public string PropertyA { get; set; }
        }

        public interface IDummyInterface : IDummyInterfaceBase
        {
            public int PropertyB { get; set; }
        }

        class DummyTypeA : IDummyInterface
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

        class DummyTypeB : IDummyInterface
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


        interface INestedType
        {
            IInnerType InnerType { get; }
        }

        interface IInnerType
        {
            string Field { get; }
        }

        class NestedTypeA : INestedType
        {
            public IInnerType InnerType { get; set; }
        }

        class InnerTypeA : IInnerType
        {
            public string Field { get; set; }
        }

        class NestedTypeB : INestedType
        {
            public InnerTypeB InnerType { get; set; }
            IInnerType INestedType.InnerType => InnerType;
        }

        class InnerTypeB : IInnerType
        {
            public string Field { get; set; }
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

        [Fact]
        public void CanConvertTypesThroughInterface()
        {
            var mapper = new ReflectionObjectMapper();

            IDummyInterface typeA = new DummyTypeA { PropertyA = "TEST STRING", PropertyB = 123 };
            IDummyInterface typeB = new DummyTypeB();

            mapper.CopyAllProperties<IDummyInterface, IDummyInterface>(typeA, typeB);

            Assert.IsType<DummyTypeB>(typeB);
            Assert.Equal(typeA.PropertyA, typeB.PropertyA);
            Assert.Equal(typeA.PropertyB, typeB.PropertyB);

        }

        [Fact]
        public void CanMapNestedObjects()
        {
            var mapper = new ReflectionObjectMapper();
            var nestedTypeA = new NestedTypeA();
            nestedTypeA.InnerType = new InnerTypeA { Field = "TEST123" };

            var nestedTypeB = mapper.CopyIntoNew<NestedTypeA,NestedTypeB>(nestedTypeA,
                new Dictionary<Type, Type>
                {
                    {  typeof(InnerTypeA), typeof(InnerTypeB) }
                });

            Assert.IsType<InnerTypeB>(nestedTypeB.InnerType);
            Assert.Equal(nestedTypeB.InnerType.Field, nestedTypeA.InnerType.Field);
        }

        [Fact]
        public void CanMapNestedObjectsByInterface()
        {
            var mapper = new ReflectionObjectMapper();
            var nestedTypeA = new NestedTypeA();
            nestedTypeA.InnerType = new InnerTypeA { Field = "TEST123" };

            var nestedTypeB = mapper.CopyIntoNew<NestedTypeA, NestedTypeB>(nestedTypeA,
                new Dictionary<Type, Type>
                {
                    {  typeof(IInnerType), typeof(InnerTypeB) }
                });

            Assert.IsType<InnerTypeB>(nestedTypeB.InnerType);
            Assert.Equal(nestedTypeB.InnerType.Field, nestedTypeA.InnerType.Field);
        }

    }
}
