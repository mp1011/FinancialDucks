﻿using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Storage;
using System.ComponentModel;

namespace FinancialDucks.Client2.Components
{
    public partial class TransactionsFilterToolbar : IAsyncDisposable
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public bool IncludeTransfersCategory { get; set; } = true;

        [Parameter]
        public bool IncludeIntervalSelector { get; set; } = false;

        [Parameter]
        public bool IncludeCategorySelector { get; set; } = true;

        [Parameter]
        public bool IncludeTextFilter { get; set; } = true;

        [Parameter]
        public TransactionSortColumn SortColumn { get; set; }

        [Parameter]
        public SortDirection SortDirection { get; set; }

        [Parameter]
        public EventCallback<TransactionsFilter> OnFilterChanged { get; set; }

        [Parameter]
        public EventCallback<TimeInterval> OnTimeIntervalChanged { get; set; }

        [Parameter]
        public TransactionsFilter CurrentFilter { get; set; }

        public DateTime RangeStart { get; set; } = DateTime.Now.AddMonths(-6);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public ITransactionSource[] Sources { get; set; }

        public string TextFilter { get; set; }

        public ICategoryDetail CategoryFilter { get; set; }

        public async Task OnCategorySelected(ICategoryDetail category)
        {
            CategoryFilter = category;
            await UpdateCurrentFilter();
        }

        public async Task OnIncludeTransfersChanged(bool includeTransfers)
        {
            IncludeTransfersCategory = includeTransfers;
            await UpdateCurrentFilter();
        }

        protected override void OnParametersSet()
        {
            if(CurrentFilter != null)
            {
                RangeStart = CurrentFilter.RangeStart;
                RangeEnd = CurrentFilter.RangeEnd;
                TextFilter = CurrentFilter.TextFilter;
                CategoryFilter = CurrentFilter.Category;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var settings = await Mediator.Send(new UserPreferencesFeature.LoadUserPreferencesQuery());
            RangeStart = settings.RangeStart;
            RangeEnd = settings.RangeEnd;
        }

        private async Task UpdateCurrentFilter(bool forceUpdate = false)
        {
            if (forceUpdate
                || CurrentFilter == null
                || CurrentFilter.RangeStart != RangeStart
                || CurrentFilter.RangeEnd != RangeEnd
                || CurrentFilter.TextFilter != TextFilter
                || CurrentFilter.Category != CategoryFilter
                || CurrentFilter.IncludeTransfers != IncludeTransfersCategory)
            {
                CurrentFilter = new TransactionsFilter(
                    RangeStart, RangeEnd, CategoryFilter, Sources, IncludeTransfersCategory, TextFilter);
                StateHasChanged();

                await OnFilterChanged.InvokeAsync(CurrentFilter);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await UpdateCurrentFilter();
        }

        public async Task OnSourcesChanged(ITransactionSourceDetail[] selectedSources)
        {
            Sources = selectedSources;
            await UpdateCurrentFilter(forceUpdate: true);
        }

        public async ValueTask DisposeAsync()
        {
            await Mediator.Send(new UserPreferencesFeature.SaveUserPreferencesCommand(
                new UserPreferences(RangeStart: RangeStart, RangeEnd: RangeEnd)
                ));
        }
    }
}
