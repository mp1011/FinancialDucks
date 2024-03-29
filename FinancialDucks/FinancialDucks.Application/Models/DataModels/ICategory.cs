﻿using FinancialDucks.Application.Models.AppModels;
using System.Text;

namespace FinancialDucks.Application.Models
{
    public interface ICategory : IWithId, IWithName
    {
        bool Starred { get; }
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

        public static void DebugWrite(this ICategoryDetail category)
        {
            var sb = new StringBuilder();
            category.DebugWrite(sb,0);
            System.Diagnostics.Debug.WriteLine(sb.ToString());
        }

        private static void DebugWrite(this ICategoryDetail category, StringBuilder sb, int indent)
        {
            sb.AppendLine(category.Name.PadLeft(category.Name.Length + indent, '\t'));
            foreach (var child in category.Children)
                child.DebugWrite(sb, indent + 1);
        }

        public static bool IsSpecialCategory(this ICategory category)
        {
            if (category == null)
                return false;

            return Enum.TryParse<SpecialCategory>(category.Name, out _);
        }

        public static ICategoryDetail Root(this ICategoryDetail category)
        {
            if (category == null || category.Parent == null)
                return new EmptyCategory();

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

        public static IEnumerable<ICategoryDetail> GetThisAndChildren(this ICategoryDetail category)
        {
            yield return category;
            foreach (var d in category.Children)
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
            if(category1 == null || category2 == null)
                return false;

            return (category1.Id == category2.Id)
                || (category1.IsAncestorOf(category2))
                || (category2.IsAncestorOf(category1));
        }

        public static bool IsDescendantOf(this ICategoryDetail category, string ancestorName)
        {
            var parent = category.Parent;
            while(parent != null)
            {
                if (parent.Name == ancestorName)
                    return true;
                else
                    parent = parent.Parent;
            }

            return false;
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

        public static IEnumerable<ICategoryDetail> GetPathFromRoot(this ICategoryDetail category)
        {
            List<ICategoryDetail> path = new List<ICategoryDetail>();
            path.Add(category);
            while(category != null)
            {
                category = category.Parent;
                if(category != null)
                    path.Add(category);
            }

            path.Reverse();
            return path;
        }
    }
}
