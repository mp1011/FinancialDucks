﻿using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Client2.Services;
using FinancialDucks.Infrastructure.Models;
using FinancialDucks.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Runtime.Versioning;

namespace FinancialDucks.Client2
{
    [SupportedOSPlatform("windows")]
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddBlazorWebViewDeveloperTools();

            var mediatorConfig = new MediatRServiceConfiguration();
            mediatorConfig.RegisterServicesFromAssemblyContaining<ReadLocalTransactions>();
            builder.Services.AddDbContext<FinancialDucksContext>();
            builder.Services.AddMediatR(mediatorConfig);
            builder.Services.AddSingleton<IObjectMapper, ReflectionObjectMapper>();
            builder.Services.AddSingleton<IDataContextProvider, DataContextProvider>();
            builder.Services.AddSingleton<DataContextProvider>();
            builder.Services.AddSingleton<IEqualityComparer<ITransaction>, TransactionEqualityComparer>();
            builder.Services.AddSingleton<ITransactionReader, TransactionReader>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();
            builder.Services.AddSingleton<ITransactionClassifier, TransactionClassifier>();
            builder.Services.AddSingleton<SqlCategoryTreeProvider>();
            builder.Services.AddSingleton<NavigationService>();
            builder.Services.AddSingleton<IScraperService, ScraperService>();
            builder.Services.AddSingleton<IBrowserAutomationService, PuppeteerService>();
            builder.Services.AddSingleton<ISecureStringSaver, SecureStringSaver>();
            builder.Services.AddSingleton<IExcelToCSVConverter, Word97ExcelConverter>();
            builder.Services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
            builder.Services.AddSingleton<IPromptEngine, OllamaPromptEngine>();
            builder.Services.AddScoped<Radzen.TooltipService>();
            builder.Services.AddSingleton((sp) =>
            {
                var dispatcher = sp.GetRequiredService<NotificationDispatcher<CategoryChangeNotification>>();
                var realProvider = sp.GetRequiredService<SqlCategoryTreeProvider>();
                ICategoryTreeProvider p = new CachedCategoryTreeProvider(realProvider, dispatcher);
                return p;
            });

            builder.Services.AddSingleton<NotificationDispatcher<CategoryChangeNotification>>();
            builder.Services.AddSingleton<NotificationDispatcher<AutoClassifierFeature.AutoClassifyNotification>>();
            builder.Services.AddSingleton<NotificationDispatcher<WebTransactionExtractorFeature.Notification>>();

            builder.Services.AddSingleton<ITransactionsQueryBuilder, TransactionsQueryBuilder>();
            var path = AppContext.BaseDirectory;
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile($@"{path}\appsettings.json");
            builder.Configuration.AddConfiguration(configBuilder.Build());

            return builder.Build();
        }
    }
}