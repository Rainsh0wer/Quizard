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
            
            // Uncomment the line below to create sample data for testing
            // SampleDataCreator.CreateSampleData();
        }
    }

}
