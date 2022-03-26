using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Models
{
    public interface ICategory : IWithId, IWithName
    {

    }

    public interface ICategoryDetail : ICategory
    {
        ICategoryDetail? Parent { get; }
        IEnumerable<ICategoryDetail> Children { get; }
        IEnumerable<ICategoryRule> Rules { get; }
        ICategoryDetail AddSubcategory(ICategory child);
    }

    public static class ICategoryExtensions
    {
        public static bool IsSpecialCategory(this ICategory category)
        {
            if (category == null)
                return false;

            return Enum.TryParse<SpecialCategory>(category.Name, out _);
        }

        public static IEnumerable<ICategoryDetail> GetAncestors(this ICategoryDetail category)
        {
            while (category.Parent != null)
            {
                yield return category.Parent;
                category = category.Parent;
            }
        }

        public static IEnumerable<ICategoryDetail> GetThisAndAllDescendants(this ICategoryDetail category)
        {
            yield return category;
            foreach(var d in category.GetDescendants())
                yield return d;
        }

        public static IEnumerable<ICategoryDetail> GetDescendants(this ICategoryDetail category)
        {
            foreach(var child in category.Children)
            {
                yield return child;

                foreach(var descendant in child.GetDescendants())
                    yield return descendant;
            }
        }

        public static ICategoryDetail? GetDescendant(this ICategoryDetail category, string name)
        {
            return category.GetDescendants().FirstOrDefault(x => x.Name == name);
        }

        public static ICategoryDetail? GetDescendant(this ICategoryDetail category, int id)
        {
            return category.GetDescendants().FirstOrDefault(x => x.Id == id);
        }
    }
}
