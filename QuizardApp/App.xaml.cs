using System.Windows;
using QuizardApp.TestData;

namespace QuizardApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Create comprehensive data for testing
            try
            {
                ComprehensiveDataCreator.CreateComprehensiveData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating data: {ex.Message}");
            }
        }
    }

}
