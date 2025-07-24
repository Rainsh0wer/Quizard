using System;
using System.Windows.Controls;

namespace QuizardApp.Services
{
    public class NavigationService
    {
        private readonly Action<UserControl> _navigate;
        public NavigationService(Action<UserControl> navigate)
        {
            _navigate = navigate;
        }
        public void Navigate(UserControl view)
        {
            _navigate(view);
        }
    }
}