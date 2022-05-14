using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Client.Models
{
    public class SyncStatusViewModel
    {
        private readonly SyncStatus _status;

        public ITransactionSourceDetail Source => _status.Account;
        public string AccountName => _status.Account.Name;

        public int AccountId => _status.Account.Id;

        public DateTime? LastTransactionDate => _status.LastTransactionDate;

        public int DownloadedTransactionCount => _status.DownloadedTransactions.Length;

        public FetchStatus FetchStatus { get; set; } = FetchStatus.NotStarted;

        public DateTime? FirstDownloadedDate { get; }
        public DateTime? LastDownloadedDate { get;  }   

        public bool DoFetch { get; set; } 

        public string FetchMessage { get; set; } = string.Empty;
        public SyncStatusViewModel(SyncStatus status)
        {
            _status = status;
            DoFetch = DownloadedTransactionCount == 0;

            if (_status.DownloadedTransactions.Any())
            {
                var t = _status.DownloadedTransactions.OrderBy(p => p.Date);
                FirstDownloadedDate = t.First().Date;
                LastDownloadedDate = t.Last().Date;
            }
        }
    }
}
