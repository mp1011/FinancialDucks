using FinancialDucks.Application.Models;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class CategoryFilter
    {
        public record CategorySelection(ICategoryDetail Parent, ICategoryDetail? SelectedChild);

        public string Id { get; private set; }

        [Parameter]
        public EventCallback<ICategoryDetail> OnCategorySelected { get; set; }

        [Parameter]
        public ICategoryDetail SelectedCategory { get; set; }

        public ICategoryDetail[] StarredCategories { get; private set; }

        public List<CategorySelection> CategorySelections { get; private set; } = new List<CategorySelection>();

        public async Task SetCategory(CategorySelection selection, ICategoryDetail category)
        {
            var index = CategorySelections.IndexOf(selection);
            CategorySelections[index] = new CategorySelection(selection.Parent, category);

            if (index < CategorySelections.Count - 1)
                CategorySelections = CategorySelections.Take(index + 1).ToList();

            if (category != null && category.Children.Any())
                CategorySelections.Add(new CategorySelection(category, null));

            if (category == null)
                await OnCategorySelected.InvokeAsync(selection.Parent);
            else
                await OnCategorySelected.InvokeAsync(category);
        }

        protected override async Task OnInitializedAsync()
        {
            Id = $"CategorySelection_{Guid.NewGuid}";
            var tree = await CategoryTreeProvider.GetCategoryTree();

            CategorySelections.Clear();
            CategorySelections.Add(new CategorySelection(tree, null));

            StarredCategories = tree.GetDescendants()
                                    .Where(p => p.Starred)
                                    .ToArray();
        }

        protected override void OnParametersSet()
        {
            if (SelectedCategory == null)
                return;

            var lastSelection = CategorySelections.LastOrDefault();
            if (lastSelection != null)
            {
                if (SelectedCategory.Id == lastSelection.Parent.Id)
                    return;

                if (lastSelection.SelectedChild != null && SelectedCategory.Id == lastSelection.SelectedChild.Id)
                    return;
            }

            CategorySelections.Clear();
            ICategoryDetail lastCategory = null;
            foreach (var category in SelectedCategory.GetPathFromRoot())
            {
                if (lastCategory == null)
                    lastCategory = category;
                else
                {
                    if (category != null)
                        CategorySelections.Add(new CategorySelection(lastCategory, category));

                    lastCategory = category;
                }
            }

        }
    }
}

