namespace FinancialDucks.Application.Models.AppModels
{
    public class EmptyCategory : ICategoryDetail
    {
        public const string Name = "None";

        public ICategoryDetail? Parent => null;

        public IEnumerable<ICategoryDetail> Children => Array.Empty<ICategoryDetail>();

        public IEnumerable<ICategoryRule> Rules => Array.Empty<ICategoryRule>();

        public bool Starred => false;

        public int Id => 0;

        string IWithName.Name => Name;

        public ICategoryDetail AddSubcategory(ICategory child)
        {
            return this;
        }
    }
}
