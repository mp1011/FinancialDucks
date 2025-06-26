using Microsoft.JSInterop;

namespace FinancialDucks.Client2.Helpers
{
    public static class JsRuntimeExtensions
    {
        public static async Task ShowModal(this IJSRuntime js, string id)
        {
            await js.InvokeVoidAsync("showModal", id);
        }
    }
}
