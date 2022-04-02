namespace FinancialDucks.Application.Models.AppModels
{
    public class ChangeTracked<T>
    {
        private T? _oldValue = default(T);

        public T? Value { get; set; }


        public void AcceptChanges()
        {
            _oldValue = Value;
        }

        public bool HasChanges => Value != null && !Value.Equals(_oldValue);

        public static implicit operator T(ChangeTracked<T> c) => c.Value;
    }
}
