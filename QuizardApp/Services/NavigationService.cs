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
            try
            {
                if (_navigationWindow?.NavigationService != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigating to: {page.GetType().Name}");
                    _navigationWindow.NavigationService.Navigate(page);
                    System.Diagnostics.Debug.WriteLine("Navigation successful");
                }
                else
                {
                    string errorMsg = $"Navigation failed: NavigationWindow is {(_navigationWindow == null ? "null" : "not null")}, NavigationService is {(_navigationWindow?.NavigationService == null ? "null" : "not null")}";
                    System.Diagnostics.Debug.WriteLine(errorMsg);
                    MessageBox.Show(errorMsg, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Navigation exception: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"{errorMsg}\nFull exception: {ex}");
                MessageBox.Show(errorMsg, "Navigation Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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