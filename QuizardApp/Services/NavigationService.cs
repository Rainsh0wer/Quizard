using System;
using System.Windows;
using System.Windows.Navigation;

namespace QuizardApp.Services
{
    public class NavigationService
    {
        private static NavigationService _instance;
        public static NavigationService Instance => _instance ??= new NavigationService();

        private NavigationWindow _navigationWindow;

        public void Initialize(NavigationWindow navigationWindow)
        {
            _navigationWindow = navigationWindow;
        }

        public void Navigate(object page)
        {
            if (_navigationWindow?.NavigationService != null)
            {
                _navigationWindow.NavigationService.Navigate(page);
            }
        }

        public void GoBack()
        {
            if (_navigationWindow?.NavigationService?.CanGoBack == true)
            {
                _navigationWindow.NavigationService.GoBack();
            }
        }

        public bool CanGoBack => _navigationWindow?.NavigationService?.CanGoBack ?? false;
    }
}