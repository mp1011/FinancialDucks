using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionCategory
    {
        public int TransactionId { get; set; }
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Transaction Transaction { get; set; }
    }
}
