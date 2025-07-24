using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using QuizardApp.ViewModels;

namespace QuizardApp.Views
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public static readonly StringToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleToDashboardConverter : IValueConverter
    {
        public static readonly RoleToDashboardConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                return role switch
                {
                    "Student" => AppState.StudentDashboard,
                    "Teacher" => AppState.TeacherDashboard,
                    _ => AppState.Login
                };
            }
            return AppState.Login;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewModelTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element)
            {
                return item switch
                {
                    LoginViewModel => element.FindResource("LoginViewTemplate") as DataTemplate,
                    RegisterViewModel => element.FindResource("RegisterViewTemplate") as DataTemplate,
                    StudentDashboardViewModel => element.FindResource("StudentDashboardTemplate") as DataTemplate,
                    TeacherDashboardViewModel => element.FindResource("TeacherDashboardTemplate") as DataTemplate,
                    TakeQuizViewModel => element.FindResource("TakeQuizTemplate") as DataTemplate,
                    CreateQuizViewModel => element.FindResource("CreateQuizTemplate") as DataTemplate,
                    ViewResultsViewModel => element.FindResource("ViewResultsTemplate") as DataTemplate,
                    SearchSubjectsViewModel => element.FindResource("SearchSubjectsTemplate") as DataTemplate,
                    JoinClassViewModel => element.FindResource("JoinClassTemplate") as DataTemplate,
                    ViewClassesViewModel => element.FindResource("ViewClassesTemplate") as DataTemplate,
                    QuizDetailsViewModel => element.FindResource("QuizDetailsTemplate") as DataTemplate,
                    _ => null
                };
            }
            return null;
        }
    }
}