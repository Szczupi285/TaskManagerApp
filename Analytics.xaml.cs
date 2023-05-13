using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace TaskManagerApp
{
    /// <summary>
    /// Logika interakcji dla klasy Analytics.xaml
    /// </summary>
    public partial class Analytics : Window
    {
        public int Id { get; init; }
        public string Login { get; init; }
        public string Password { get; init; } 


        public Analytics(int id, string login, string password)
        {
            InitializeComponent();
            Id = id;
            Login = login;
            Password = password;

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            SqlCommand GetCompletedTaskCount = new SqlCommand($"Select COUNT(*) from Notes Where IsFinished='1' AND UserID = '{Id}'", connection);
            SqlCommand GetFailedTaskCount = new SqlCommand($"Select Count(*) From Notes Where IsFinished ='0' AND (IsFailed='1' OR CONVERT(DATETIME, GETDATE(), 101) > Deadline)  AND UserID = '7'\r\n",connection);
            SqlCommand GetPendingTasks = new SqlCommand($"Select COUNT(*) From Notes Where IsFailed='0' AND IsFinished='0' AND CONVERT(DATETIME, GETDATE(), 101) < Deadline AND UserID='{Id}'", connection);

            SqlDataRecord getCompTask = new SqlDataRecord();

            int CompletedTask = Convert.ToInt32(GetCompletedTaskCount.ExecuteScalar());
            int FailedTask = Convert.ToInt32(GetFailedTaskCount.ExecuteScalar());
            int PendingTask = Convert.ToInt32(GetPendingTasks.ExecuteScalar());

            double AvgCompletion = 100.0 / (CompletedTask + FailedTask) * CompletedTask;


            CompletedTasksCount.Text = Convert.ToString(CompletedTask);
            AvgTaskCompletion.Text = Convert.ToString(AvgCompletion);
            FailedTasks.Text = Convert.ToString(FailedTask);
            PendingTasks.Text = Convert.ToString(PendingTask);



        }

        private void TaskWindow_Click(object sender, RoutedEventArgs e)
        {
            TaskWindow taskwindow = new TaskWindow(Id, Login, Password);
            this.Close();
            taskwindow.Show();
        }

        private void AnalyticsSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Checkboxes.Visibility == Visibility.Hidden)
                Checkboxes.Visibility = Visibility.Visible;
            else
                Checkboxes.Visibility = Visibility.Hidden;
        }

        
    }
}
