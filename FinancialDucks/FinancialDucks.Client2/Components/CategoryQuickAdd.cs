﻿using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
{
    public partial class CategoryQuickAdd
    {
        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public ITransaction Transaction { get; set; }

        [Parameter]
        public EventCallback<ICategoryDetail> AfterCreate { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public ICategoryDetail Category { get; set; }

        public string NewCategoryName { get; set; }

        public void NewCategoryButton_Click()
        {
            NewCategoryName = Text.AutoCapitalize();
        }

        public void Cancel()
        {
            NewCategoryName = null;
        }

        public async Task SaveChanges()
        {
            var result = await Mediator.Send(new CategoryQuickAddFeature.Command(Text, Category, NewCategoryName));
            await AfterCreate.InvokeAsync(result);
            NewCategoryName=null;
        }
    }
}
