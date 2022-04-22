using FinancialDucks.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace FinancialDucks.Client.Services
{
    internal class NavigationService
    {
        private readonly Stack<NavigationFrame> _navigationFrames = new Stack<NavigationFrame>();
        private NavigationManager? _navigationManager;
       
        public void Initialize(NavigationManager navigationManager)
        {
            if (_navigationManager != null)
                return;

            navigationManager.LocationChanged += _navigationManager_LocationChanged;
            _navigationManager=navigationManager;
            _navigationFrames.Push(new NavigationFrame(_navigationManager.Uri));
        }

        private void _navigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            if (_navigationFrames.Peek().Page == e.Location)
                return;

            _navigationFrames.Push(new NavigationFrame(e.Location));
        }

        public bool CanGoBack => _navigationFrames.Count > 1;

        public void GoBack()
        {
            _navigationFrames.Pop();
            _navigationManager.NavigateTo(_navigationFrames.Peek().Page);
        }
    }
}
