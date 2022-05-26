using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using System.Collections.Generic;

namespace FinancialDucks.Tests.TestModels
{
    internal class TestCategory : ICategoryDetail
    {
        public override string ToString()
        {
            return Name;
        }
        
        public int Id { get; }

        public string Name { get; }

        public bool Starred { get; set; }

        public TestCategory(int id, string name, TestCategory? parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
        }

        public TestCategory? Parent { get; set;  }

        public List<TestCategory> Children { get; } = new List<TestCategory>();

        ICategoryDetail? ICategoryDetail.Parent => Parent;

        IEnumerable<ICategoryDetail> ICategoryDetail.Children => Children;

        IEnumerable<ICategoryRule> ICategoryDetail.Rules => throw new System.NotImplementedException();

        ICategoryDetail ICategoryDetail.AddSubcategory(ICategory child)
        {
            return AddSubcategory(child);
        }

        public TestCategory AddSubcategory(ICategory child)
        {
            var childCategory = new TestCategory(child.Id, child.Name, this);
            Children.Add(childCategory);
            return childCategory;
        }

        public TestCategory AddChild(int id, string name)
        {
            return AddSubcategory(new Category(id, name, starred:false, parent:null));
        }

        public TestCategory AddChildReturnThis(int id, string name)
        {
            AddSubcategory(new Category(id, name, false, parent:null));
            return this;
        }
    }
}
