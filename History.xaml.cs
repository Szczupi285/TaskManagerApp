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
using TaskManagerApp;

namespace TaskManagerApp
{
    /// <summary>
    /// Logika interakcji dla klasy History.xaml
    /// </summary>
    public partial class History : Window
    {
        public int Id { get; init; }
        public string Login { get; init; }
        public string Password;

        // to do change value in database in setters
        public bool ShowAllHistory { get; set; }
        public bool ShowFinishedHistory { get; set; }
        public bool ShowUnfinishedHistory { get; set; }

        public History(int id, string login , string password)
        {
            InitializeComponent();
            this.Id = id;
            this.Login = login;
            this.Password = password;
            LoadHistoryFromDB();
        }
        public void OpenAndCloseConn(Action<SqlConnection> action)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                action(connection);
                connection.Close();
            }
        }

        private void LoadHistoryFromDB()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand ReadNotes;

            if(ShowAllHistory)
                ReadNotes = new SqlCommand($"SELECT Note, Deadline, IsFinished FROM Notes WHERE UserID = '{Id}' " +
                    $"AND (Deadline < CONVERT(DATETIME, GETDATE(), 101) OR IsFinished = '1' OR IsFailed = '1')", connection);
            else if(ShowFinishedHistory)
            {
                ReadNotes = new SqlCommand($"SELECT Note, Deadline FROM Notes WHERE UserID = '{Id}' " +
                    $"AND IsFinished = '1'", connection);
            }
            else
            {
                ReadNotes = new SqlCommand($"SELECT Note, Deadline FROM Notes WHERE UserID = '{Id}' AND IsFinished = '0'" +
                    $"AND (IsFailed='1' OR CONVERT(DATETIME, GETDATE(), 101) > Deadline)", connection);

            }
            
            SqlDataReader readerNotes = ReadNotes.ExecuteReader();
            



            // for record in database that date hasn't expired yet
            int iterator = 0;
            while (readerNotes.Read())
            {

                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Auto);
                CurrentNotes.RowDefinitions.Add(rowDef);

                TextBlock Note = new TextBlock();

                // read value from database
                Note.Text = $"Task: {readerNotes["Note"]} \nDeadline: {((DateTime)readerNotes["Deadline"]).ToString("yyyy/MM/dd")}";
                // assing the id of note to FinBtn name so we get the id of the exact note next to button
                // we need to append "A" because Name of the button can't be only the number
                

                Note.Margin = new Thickness(10);
                Note.TextWrapping = TextWrapping.Wrap;

                // creates the frame and assign the properties
                Frame frame = new Frame();
                frame.Content = Note;
                frame.Margin = new Thickness(10);
                frame.BorderThickness = new Thickness(2);
                frame.BorderBrush = Brushes.Black;

                // display note with green backgroud if the task was finished and red if it wasn't
                
                if (ShowAllHistory)
                {
                    int isFinished = Convert.ToInt32(readerNotes["IsFinished"]);
                    if (isFinished == 1)
                        frame.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                    else
                    {
                        frame.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                    }
                }
                else if(ShowFinishedHistory)
                {
                    frame.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                }
                else
                {
                    frame.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                }


                Grid.SetRow(frame, iterator);
                

                CurrentNotes.Children.Add(frame);
                

                iterator++;
            }


            connection.Close();
        }

        #region settings
        private void HistorySettings_Click(object sender, RoutedEventArgs e)
        {
            if (Checkboxes.Visibility == Visibility.Hidden)
                Checkboxes.Visibility = Visibility.Visible;
            else
                Checkboxes.Visibility = Visibility.Hidden;
        }

        private void TaskWindow_Click(object sender, RoutedEventArgs e)
        {
            
            TaskWindow taskwindow = new TaskWindow(Id, Login, Password);
            this.Close();
            taskwindow.Show();

        }



        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set ShowAllHis = '1', ShowFinHis = '0'," +
                    $" ShowUnfinHis = '0' Where UserId = '{Id}'", connection);

                ShowAllHistory = true;
                ShowFinishedHistory = false;
                ShowUnfinishedHistory = false;
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set ShowAllHis = '0', ShowFinHis = '1'," +
                    $" ShowUnfinHis = '0' Where UserId = '{Id}'", connection);

                ShowAllHistory = false;
                ShowFinishedHistory = true;
                ShowUnfinishedHistory = false;

                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set ShowAllHis = '0', ShowFinHis = '0', " +
                    $"ShowUnfinHis = '1' Where UserId = '{Id}'", connection);

                ShowAllHistory = false;
                ShowFinishedHistory = false;
                ShowUnfinishedHistory = true;

                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);   
        }

        #endregion


        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            CurrentNotes.Children.Clear();
            LoadHistoryFromDB();
        }
    }
}
