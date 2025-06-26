namespace FinancialDucks.Client2.Models
{
    public class LabelledData<T>
    {
        public string Label { get; }
        public decimal Value { get; }
        public T Data { get; }

        public LabelledData(string label, decimal value, T data)
        {
            Label = label;
            Value = value;
            Data = data;
        }
    }
}
