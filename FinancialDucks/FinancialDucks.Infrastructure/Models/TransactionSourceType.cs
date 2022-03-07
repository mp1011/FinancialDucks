using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSourceType
    {
        public TransactionSourceType()
        {
            TransactionSources = new HashSet<TransactionSource>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<TransactionSource> TransactionSources { get; set; }
    }
}
