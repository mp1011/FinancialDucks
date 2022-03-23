﻿using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class ImportFeatureTests : TestBase 
    {

        [Fact]
        public async Task CanImportAllTransactionsInFolder()
        {
            var serviceProvider = CreateServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var result = await mediator!.Send(new ReadLocalTransactions.Request());

            Assert.Equal(-13441.22M, result.Sum(p => p.Amount));
        }
    }
}
