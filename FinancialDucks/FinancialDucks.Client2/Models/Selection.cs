namespace FinancialDucks.Client2.Models
{
    public interface ISelection
    {
        bool Selected { get; set; }
    }

    public class Selection<T> :ISelection
    {
        public T Data { get; }
        public bool Selected { get; set; }

        public Selection(T data, bool selected)
        {
            Data = data;
            Selected = selected;
        }
    }

    public static class SelectionExtensions
    {
        public static T[] GetSelectedData<T>(this IEnumerable<Selection<T>> selection)
        {
            return selection
                .Where(p => p.Selected)
                .Select(p => p.Data)
                .ToArray();
        }
    }
}
