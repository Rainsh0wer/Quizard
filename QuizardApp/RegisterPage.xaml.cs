using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuizardApp.ViewModels;

namespace QuizardApp
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            this.DataContext = new RegisterViewModel();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (RegisterViewModel)this.DataContext;
            vm.Password = passwordBox.Password;
            vm.ConfirmPassword = confirmPasswordBox.Password;

            if (vm.RegisterCommand.CanExecute(null))
            {
                vm.RegisterCommand.Execute(null);

                if (vm.Message.StartsWith("Registration successful"))
                {
                    MessageBox.Show("Đăng ký thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Đăng ký thất bại: " + vm.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
