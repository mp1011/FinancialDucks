using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialDucks.Client.Models
{
    public class Selection<T>
    {
        public T Data { get; }
        public bool Selected { get; set; }

        public Selection(T data, bool selected)
        {
            Data = data;
            Selected = selected;
        }
    }
}
