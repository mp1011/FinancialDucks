﻿using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using System.Linq.Expressions;

namespace FinancialDucks.Application.Features
{
    public class CategoryStatsFeature
    {
        public record Query(ICategory Category) : IRequest<CategoryStats> { }

        public class Handler : IRequestHandler<Query, CategoryStats>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public Handler(IDataContextProvider dataContextProvider, ICategoryTreeProvider categoryTreeProvider)
            {
                _dataContextProvider = dataContextProvider;
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();

                var requestCategory = categories.GetDescendant(request.Category.Id);
                if(requestCategory == null)
                        return new CategoryStats(0, 0, new DescriptionWithCount[] { });

                if (request.Category.Name == SpecialCategory.Debits.ToString())
                    return await GetTotalStats(requestCategory, p=>p.Amount < 0);

                if (request.Category.Name == SpecialCategory.Credits.ToString())
                    return await GetTotalStats(requestCategory, p=>p.Amount > 0);

                int[] requestCategoryIds = requestCategory
                                        .GetThisAndAllDescendants()
                                        .Select(x => x.Id)
                                        .ToArray();

                using var ctx = _dataContextProvider.CreateDataContext();

                var query = from t in ctx.TransactionsDetail
                            from cr in ctx.CategoryRulesDetail
                            where requestCategoryIds.Contains(cr.CategoryId)
                                && cr.SubstringMatch != null
                                && t.Description.Contains(cr.SubstringMatch)
                            select new { Transaction = t, Category = cr.Category };

                var result = await query
                     .DebugWatchCommandText(ctx)
                     .ToArrayAsync(ctx);

                var descriptionCounts = result
                    .Select(p => p.Transaction.Description.CleanNumbersAndSpecialCharacters())
                    .GroupBy(g => g)
                    .Select(g => new DescriptionWithCount(g.Key, g.Count()))
                    .OrderByDescending(p => p.Count)
                    .ToArray();

                return new CategoryStats(result.Count(), result.Sum(p => p.Transaction.Amount), descriptionCounts);
            }
        
        
            private async Task<CategoryStats> GetTotalStats(ICategoryDetail category, Expression<Func<ITransactionDetail,bool>> condition)
            {
                using var ctx = _dataContextProvider.CreateDataContext();

                var transferCategoryIds = category.Root()
                                                  .GetDescendant(SpecialCategory.Transfers.ToString())!
                                                  .GetThisAndAllDescendants()
                                                  .Select(p => p.Id)
                                                  .ToArray();

                var tranferTransactions = from t in ctx.TransactionsDetail
                                          from cr in ctx.CategoryRulesDetail
                                          where transferCategoryIds.Contains(cr.CategoryId)
                                              && cr.SubstringMatch != null
                                              && t.Description.Contains(cr.SubstringMatch)
                                          select t;

                var query = ctx.TransactionsDetail
                                .Where(condition)
                                .Except(tranferTransactions)
                                .Select(t=> new { Transaction = t, Category = category });

                var result = await query
                     .DebugWatchCommandText(ctx)
                     .ToArrayAsync(ctx);


                var queryCategorized = from t in ctx.TransactionsDetail
                                          from cr in ctx.CategoryRulesDetail
                                          where cr.SubstringMatch != null
                                              && t.Description.Contains(cr.SubstringMatch)
                                          select t;

                var queryUncategorized = ctx.TransactionsDetail
                                .Where(condition)
                                .Except(queryCategorized);

                var descriptionCounts = queryUncategorized
                    .ToArray()
                    .Select(p => p.Description.CleanNumbersAndSpecialCharacters())
                    .GroupBy(g => g)
                    .Select(g => new DescriptionWithCount(g.Key, g.Count()))
                    .OrderByDescending(p => p.Count)
                    .ToArray();

                return new CategoryStats(result.Count(), result.Sum(p => p.Transaction.Amount), descriptionCounts);
            }
        }
    }
}
