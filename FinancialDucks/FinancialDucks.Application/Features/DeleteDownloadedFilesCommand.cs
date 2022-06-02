using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public record DeleteDownloadedFilesCommand(ITransactionSource Source) : IRequest<bool>
    {
        public class Handler : IRequestHandler<DeleteDownloadedFilesCommand, bool>
        {
            private readonly ISettingsService _settingsService;

            public Handler(ISettingsService settingsService)
            {
                _settingsService = settingsService;
            }

            public Task<bool> Handle(DeleteDownloadedFilesCommand command, CancellationToken cancellationToken)
            {
                bool anyFailed = false;
                var files = GetFiles(command).ToArray();
                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        anyFailed = true;
                    }
                }

                return Task.FromResult(!anyFailed);
            }

            private IEnumerable<FileInfo> GetFiles(DeleteDownloadedFilesCommand command)
            {
                var folder = _settingsService.SourcePath;
               
                var accountFolder = new DirectoryInfo($"{folder.FullName}\\{command.Source.Name}");
                foreach(var file in accountFolder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                                                    .Where(p => p.Extension.ToLower() == ".csv" || p.Extension.ToLower() == ".xls"))
                {
                    yield return file;
                }
            }
        }
    }
}
