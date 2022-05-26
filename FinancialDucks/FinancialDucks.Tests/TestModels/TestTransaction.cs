#nullable disable
using FinancialDucks.Application.Models;
using System;

namespace FinancialDucks.Tests.TestModels
{
    internal class TestTransaction : ITransactionDetail
    {
        public ITransactionSource Source { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public int SourceId { get; set; }

        public int Id { get; set; }

        public ICategory[] Categories { get; set; }

        public override string ToString() => $"{Date.ToShortDateString()} {Description} {Amount.ToString("C")}";

    }
}
