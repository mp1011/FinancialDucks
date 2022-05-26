namespace FinancialDucks.Application.Models.AppModels
{
    public record SimpleCategory(int Id, string Name) : ICategory
    {
        public bool Starred => false;
    }

    public class Category : ICategoryDetail
    {      
        public int Id { get; }

        public string Name { get; }

        public bool Starred { get; }

        public Category(int id, string name, bool starred, Category? parent)
        {
            Id= id;
            Name= name;
            Parent = parent;
            Starred= starred;
        }

        public Category? Parent { get; }

        private List<Category> _children = new List<Category>();

        public IEnumerable<Category> Children => _children;

        public List<ICategoryRule> Rules { get; } = new List<ICategoryRule>();

        ICategoryDetail? ICategoryDetail.Parent => Parent;

        IEnumerable<ICategoryDetail> ICategoryDetail.Children => Children;

        IEnumerable<ICategoryRule> ICategoryDetail.Rules => Rules;

        ICategoryDetail ICategoryDetail.AddSubcategory(ICategory child)
        {
            var childCategory = new Category(child.Id, child.Name, starred:false, parent:this);
            AddChild (childCategory);
            return childCategory;
        }

        public void AddChild(Category child)
        {
            if (_children.Any(c => c.Name == child.Name))
                return;

            _children.Add(child);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
