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

namespace TaskManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void RegisterSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoginSend_Click(object sender, RoutedEventArgs e)
        {
            // if(login = databaseLogin && password = databasePassoword)
            // open new window
            TaskWindow window = new TaskWindow();
            window.Show();
            this.Close();
            // else wrong login or password popup
        }
    }
}
