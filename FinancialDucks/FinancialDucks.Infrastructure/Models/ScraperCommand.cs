using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class ScraperCommand
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int TypeId { get; set; }
        public int Sequence { get; set; }

        [NotMapped]
        public bool PauseBeforeStep { get; set; }
        public string Selector { get; set; }
        public string Text { get; set; }
        public int TimeoutSeconds { get; set; }

        public virtual TransactionSource Source { get; set; }
    }
}
