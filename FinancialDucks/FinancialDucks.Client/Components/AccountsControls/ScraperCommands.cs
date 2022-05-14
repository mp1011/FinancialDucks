﻿using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.AccountsControls
{
    public partial class ScraperCommands : INotificationHandler<WebTransactionExtractorFeature.Notification>, IDisposable
    {
        [Parameter]
        public ITransactionSourceDetail Source { get; set; }

        [Inject]
        public NotificationDispatcher<WebTransactionExtractorFeature.Notification> Dispatcher { get; set; } 

        [Inject]
        public IObjectMapper ObjectMapper { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        private List<ScraperCommandEdit> _commands = new List<ScraperCommandEdit>();
        private List<WebTransactionExtractorFeature.Notification> _notifications = new List<WebTransactionExtractorFeature.Notification>();

        public FileInfo[] _downloadedFiles = null;

        public IEnumerable<ScraperCommandEdit> Commands => _commands
            .NullToEmpty()
            .OrderBy(p => p.Sequence);

        public async Task Save(ScraperCommandEdit command)
        {
            var saved = await Mediator.Send(new ScraperCommandsFeature.SaveCommand(command));
            command.Id = saved.Id;

            if(_commands.Last().Id !=0)
                _commands.Add(new ScraperCommandEdit { 
                    Source = Source,
                    Sequence = _commands.Select(c => c.Sequence).MaxOrDefault() + 1 });
        }

        protected override void OnInitialized()
        {
            Dispatcher.Register(this, NotificationPriority.Low);
        }



        protected override async Task OnParametersSetAsync()
        {
            if (Source == null)
                return;

            _commands.Clear();
            var commands = await Mediator.Send(new ScraperCommandsFeature.Query(Source));
            _commands.AddRange(commands.Select(p => ObjectMapper.CopyIntoNew<IScraperCommandDetail, ScraperCommandEdit>(p)));

            _commands.Add(new ScraperCommandEdit {
                Source = Source,
                Sequence = _commands.Select(c=>c.Sequence).MaxOrDefault() + 1 });
        }

        public async Task Test()
        {
            _notifications.Clear();
            var files = await Mediator.Send(new WebTransactionExtractorFeature.QueryPreview(_commands.Where(p=>!p.IsDefault).ToArray()));
            _downloadedFiles = files;
        }

        public void Dispose()
        {
            Dispatcher.Unregister(this);
        }

        Task INotificationHandler<WebTransactionExtractorFeature.Notification>.Handle(WebTransactionExtractorFeature.Notification notification, CancellationToken cancellationToken)
        {
            _notifications.Add(notification);
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
