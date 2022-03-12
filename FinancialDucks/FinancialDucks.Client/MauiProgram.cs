using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Infrastructure.Models;
using MediatR;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;

namespace FinancialDucks.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .RegisterBlazorMauiWebView()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddBlazorWebView();

            builder.Services.AddDbContext<FinancialDucksContext>();
            builder.Services.AddMediatR(typeof(ReadLocalTransactions));
            builder.Services.AddSingleton<IObjectMapper, ReflectionObjectMapper>();
            builder.Services.AddSingleton(s => s.GetService<FinancialDucksContext>() as IDataContext);
            builder.Services.AddSingleton<IEqualityComparer<ITransaction>, TransactionEqualityComparer>();
            builder.Services.AddSingleton<ITransactionReader, TransactionReader>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();

            var path = AppContext.BaseDirectory;
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile($@"{path}\appsettings.json");
            builder.Configuration.AddConfiguration(configBuilder.Build());


            return builder.Build();
        }
    }
}