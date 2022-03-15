namespace FinancialDucks.Application.Models.AppModels
{
    public class Category : ICategoryDetail
    {      
        public int Id { get; }

        public string Name { get; }

        public Category(int id, string name, Category? parent)
        {
            Id= id;
            Name= name;
            Parent = parent;
        }

        public Category? Parent { get; }

        public List<Category> Children { get; } = new List<Category>();

        public List<ICategoryRule> Rules { get; } = new List<ICategoryRule>();

        ICategoryDetail? ICategoryDetail.Parent => Parent;

        IEnumerable<ICategoryDetail> ICategoryDetail.Children => Children;

        IEnumerable<ICategoryRule> ICategoryDetail.Rules => Rules;

        ICategoryDetail ICategoryDetail.AddSubcategory(ICategory child)
        {
            var childCategory = new Category(child.Id, child.Name, this);
            Children.Add(childCategory);
            return childCategory;
        }

        
    }
}
