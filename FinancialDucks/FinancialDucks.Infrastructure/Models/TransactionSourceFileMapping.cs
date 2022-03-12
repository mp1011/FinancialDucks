using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSourceFileMapping
    {
        public int Id { get; set; }
        public string FilePattern { get; set; }
        public int SourceId { get; set; }

        public virtual TransactionSource Source { get; set; }
    }
}
