using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialDucks.Application.Models.AppModels
{
    public class AccountEdit : ITransactionSourceDetail
    {
        public IEnumerable<ISourceSnapshot> SourceSnapshots { get; }

        public ITransactionSourceType SourceType { get; }

        public int TypeId { get; set; }

        public string Url { get; set; }

        public int Id { get; }

        public string Name { get; set; }

        public AccountEdit(ITransactionSourceDetail src)
        {
            SourceSnapshots = src.SourceSnapshots;
            SourceType = src.SourceType;
            TypeId = src.TypeId;
            Url = src.Url;
            Id = src.Id;
            Name = src.Name;
        }
    }
}
