using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionCategory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int SourceId { get; set; }
        public int? CategoryId { get; set; }
        public string? Category { get; set; }
    }
}
