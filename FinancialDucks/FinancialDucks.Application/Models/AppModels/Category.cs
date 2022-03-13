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

        ICategoryDetail? ICategoryDetail.Parent => Parent;

        IEnumerable<ICategoryDetail> ICategoryDetail.Children => Children;

        public Category AddChild(int id, string Name)
        {
            var child = new Category(id, Name, this);
            Children.Add(child);
            return child;
        }

    }
}
