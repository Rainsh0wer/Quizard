using System;
using System.Windows;
using System.Windows.Navigation;

namespace QuizardApp.Services
{

    public class AppNavigationService
    {
        private static AppNavigationService _instance;
        public static AppNavigationService Instance => _instance ??= new AppNavigationService();


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