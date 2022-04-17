using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class SourceSnapshot
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public virtual TransactionSource Source { get; set; }
    }
}
