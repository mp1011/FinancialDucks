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

        public static ICategoryDetail Root(this ICategoryDetail category)
        {
            var node = category;
            while(node.Parent != null)
                node = node.Parent;

            return node;
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
            if(name == category.Name)
                return category; 

            return category.GetDescendants().FirstOrDefault(x => x.Name == name);
        }

        public static ICategoryDetail? GetDescendant(this ICategoryDetail category, int id)
        {
            return category.GetDescendants().FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Returns true if the categories are the same, or one is the ancestor of the other
        /// </summary>
        /// <param name="category1"></param>
        /// <param name="category2"></param>
        /// <returns></returns>
        public static bool HasLinearRelationTo(this ICategoryDetail category1, ICategoryDetail category2)
        {
            return (category1.Id == category2.Id)
                || (category1.IsAncestorOf(category2))
                || (category2.IsAncestorOf(category1));
        }

        public static bool IsAncestorOf(this ICategoryDetail category1, ICategoryDetail category2)
        {
            var parent = category2.Parent;
            while (true)
            {
                if (parent == null)
                    return false;
                else if (parent.Id == category1.Id)
                    return true;
                else
                    parent = parent.Parent;
            }
        }

    }
}
