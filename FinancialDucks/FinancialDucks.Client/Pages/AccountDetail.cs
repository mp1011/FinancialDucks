using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class AccountDetail
    {
        private AccountEdit _account;

        private string _loginName;
        private string _password;

        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public ISecureStringSaver SecureStringSaver { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var sources = await Mediator.Send(new TransactionSourcesFeature.Query());
            var source = sources.FirstOrDefault(s => s.Id == Id);
            if (source != null)
                _account = new AccountEdit(source);
        }

        public async Task Update()
        {
            if(_loginName.NotNullOrEmpty())
                SecureStringSaver.SaveUsername(_account, _loginName);

            if (_password.NotNullOrEmpty())
                SecureStringSaver.SavePassword(_account, _password);

            await Mediator.Send(new TransactionSourcesFeature.UpdateCommand(_account));

        }

        public void SaveLoginName(string text)
        {
            
        }

        public void SavePassword(string text)
        {
            SecureStringSaver.SavePassword(_account, text);
        }
    }
}
