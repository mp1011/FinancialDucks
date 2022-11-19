using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class BudgetLine
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public decimal Budget { get; set; }

        public virtual Category Category { get; set; }
    }
}
