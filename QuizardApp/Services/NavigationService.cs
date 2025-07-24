using System;

namespace QuizardApp.Services
{
    public class NavigationService
    {
        private readonly Action<object> _setContent;
        
        public NavigationService(Action<object> setContent)
        {
            _setContent = setContent;
        }
        
        public void SetContent(object content)
        {
            _setContent(content);
        }
    }
}