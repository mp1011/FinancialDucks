using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class CategoryRule
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string SubstringMatch { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public DateTime? DateMin { get; set; }
        public DateTime? DateMax { get; set; }
        public short Priority { get; set; }

        public virtual Category Category { get; set; }
    }
}
