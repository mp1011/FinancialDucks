using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSource
    {
        public TransactionSource()
        {
            ScraperCommands = new HashSet<ScraperCommand>();
            SourceSnapshots = new HashSet<SourceSnapshot>();
            TransactionSourceFileMappings = new HashSet<TransactionSourceFileMapping>();
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }
        public string Url { get; set; }

        public virtual TransactionSourceType Type { get; set; }
        public virtual ICollection<ScraperCommand> ScraperCommands { get; set; }
        public virtual ICollection<SourceSnapshot> SourceSnapshots { get; set; }
        public virtual ICollection<TransactionSourceFileMapping> TransactionSourceFileMappings { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
