using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSource
    {
        public TransactionSource()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }

        public virtual TransactionSourceType Type { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
