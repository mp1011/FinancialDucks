﻿using FinancialDucks.Application.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FinancialDucks.Tests.CustomMocks
{
    public class MockBrowserAutomation : IBrowserAutomation
    {
        private readonly ISettingsService _settingsService;

        public MockBrowserAutomation(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public string Url { get; set; } = "";

        public void Dispose()
        {
        }

        public Task DoClick(string selector, bool searchInnerText)
        {
            Url += "x";

            if (selector == "download")
            {
                var downloadsFolder = _settingsService.DownloadsFolder;
                File.WriteAllText(downloadsFolder.FullName + "\\test.txt", DateTime.Now.ToLongDateString());
            }

            return Task.CompletedTask;
        }

        public Task FillText(string selector, string text)
        {
            return Task.CompletedTask;
        }

        public Task WaitForNavigate(string originalUrl)
        {
            return Task.CompletedTask;
        }
    }
}
