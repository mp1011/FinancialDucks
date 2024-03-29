﻿namespace FinancialDucks.Application.Models
{
    public interface ITransaction : IWithId
    {
        decimal Amount { get; }
        DateTime Date { get; }
        string Description { get; }
        int SourceId { get; }
    }

    public interface ITransactionDetail : ITransaction
    {
        ITransactionSource Source { get; }
    }

    public interface ITransactionWithCategory : ITransaction
    {      
        int? CategoryId { get; }
        string? Category { get; }        
    }
}
