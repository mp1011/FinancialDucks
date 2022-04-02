using Microsoft.JSInterop;

namespace FinancialDucks.Client.Helpers
{
    public static class JsRuntimeExtensions
    {
        public static async Task ShowModal(this IJSRuntime js, string id)
        {
            await js.InvokeVoidAsync("showModal", id);
        }
    }
}
