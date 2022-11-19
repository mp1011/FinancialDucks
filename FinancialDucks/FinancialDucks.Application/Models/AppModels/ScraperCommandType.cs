namespace FinancialDucks.Application.Models.AppModels
{
    public enum ScraperCommandType
    {
        FillUsername,
        FillPassword,
        Click,
        ClickAndDownload,
        FillCurrentDate,
        FillPastDate,
        GoBack,
        Wait,
        SelectDropdownText,
        NavigateTo,
        KeyPress
    }
}
