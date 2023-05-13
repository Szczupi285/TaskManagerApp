using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Shell;
using System.Net.Mail;
using System.Reflection;
using System.Security.Principal;

namespace TaskManagerApp
{
    /// <summary>
    /// Logika interakcji dla klasy TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        
        public int Id { get; init; }
        public string Login { get; init; }
        public string Password { get; init; }
        // to do make settings properties and change value in database in setters



        public TaskWindow(int id, string login, string password)
        {
            this.Id = id;
            this.Login = login;
            this.Password = password;

            InitializeComponent();

           
            
            OpenAndCloseConn((connection) => {
                // initialize Settings table for user if he logs in for the first time 
                SqlCommand InitializeSettings = new SqlCommand($"INSERT INTO Settings (UserId) Select '{Id}' WHERE NOT EXISTS (Select (UserId) from Settings Where UserId = '{Id}')", connection);
                InitializeSettings.ExecuteNonQuery();
                // checks the settings values that already has been set
                // set's them up upon initialize so they have values that user set in previous session 
                SqlCommand getSettings = new SqlCommand($"SELECT DeadlineSetting, SendRemainder, SortByDeadline From Settings Where UserId = '{Id}'", connection);
                SqlDataReader SettingsRecord = getSettings.ExecuteReader();
                while (SettingsRecord.Read())
                {
                    
                    if (SettingsRecord.GetBoolean(0) == false)
                        Highlight.IsChecked = false;
                    else 
                        Highlight.IsChecked = true;


                    if (SettingsRecord.GetBoolean(1) == false)
                        Remainder.IsChecked = false;
                    else
                        Remainder.IsChecked = true;


                    if(SettingsRecord.GetBoolean(2) == false)
                        Sort.IsChecked = false;
                    else
                        Sort.IsChecked = true;
                }
                SettingsRecord.Close();
                // popup remainder
                if(Remainder.IsChecked == true)
                {
                    // shows the number of tasks that deadline is tommorow
                    SqlCommand ComingToEnd = new SqlCommand($"Select Count(Note) From Notes Where DATEDIFF(day, GETDATE(), Deadline) = 1 AND Deadline > GetDate()", connection);
                    int count = (int)ComingToEnd.ExecuteScalar();

                    if (count > 0)
                        MessageBox.Show($"You have {count} tasks coming to an end");
                }

            });

            
            
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

        private void LoadNotesFromDB()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand ReadNotes;

            if(Sort.IsChecked == true)
            {
                ReadNotes = new SqlCommand($"SELECT Note, Deadline , Id FROM Notes WHERE UserID = '{Id}' AND Deadline > CONVERT(DATETIME, GETDATE(), 101) AND IsFinished = '0' AND IsFailed = '0' Order BY Deadline ASC", connection);
            }
            else
            {
                ReadNotes = new SqlCommand($"SELECT Note, Deadline, Id FROM Notes WHERE UserID = '{Id}' AND Deadline > CONVERT(DATETIME, GETDATE(), 101) AND IsFinished = '0' AND IsFailed = '0'", connection);
                
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
                
                // creates the finish button and assign the properties
                Button FinBtn = new Button();
                FinBtn.Content = "\u2713";
                FinBtn.Width = 20;
                FinBtn.Height = 20;
                FinBtn.VerticalAlignment = VerticalAlignment.Top;
                FinBtn.HorizontalAlignment = HorizontalAlignment.Left;
                FinBtn.Click += new RoutedEventHandler(FinBtn_Click);

                // create the Fail button
                Button FailBtn = new Button();
                FailBtn.Content = "\u2717";
                FailBtn.Width = 20;
                FailBtn.Height = 20;
                FailBtn.VerticalAlignment = VerticalAlignment.Bottom;
                FailBtn.HorizontalAlignment = HorizontalAlignment.Left;
                FailBtn.Click += new RoutedEventHandler(FailBtn_Click);


                // read value from database
                Note.Text = $"Task: {readerNotes["Note"]} \nDeadline: {((DateTime)readerNotes["Deadline"]).ToString("yyyy/MM/dd")}";
                // assing the id of note to FinBtn name so we get the id of the exact note next to button
                // we need to append "A" because Name of the button can't be only the number
                var x = "A" + readerNotes["Id"].ToString();
                FinBtn.Name = x;
                FailBtn.Name = x;

                Note.Margin = new Thickness(10);
                Note.TextWrapping = TextWrapping.Wrap;

                // creates the frame and assign the properties
                Frame frame = new Frame();
                frame.Content = Note;
                frame.Margin = new Thickness(10);
                frame.BorderThickness = new Thickness(2);
                frame.BorderBrush = Brushes.Black;
                
                // gets the difference of deadline and current time
                TimeSpan difference = (DateTime)readerNotes["Deadline"] - DateTime.Now;
                // checks if deadline is less than in 3 days and if highlight setting is turned on
                if (difference.TotalDays < 3 && Highlight.IsChecked == true)
                    frame.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

                Grid.SetRow(frame, iterator);
                Grid.SetRow(FinBtn, iterator);
                Grid.SetRow(FailBtn, iterator);

                CurrentNotes.Children.Add(frame);
                CurrentNotes.Children.Add(FinBtn);
                CurrentNotes.Children.Add(FailBtn);

                iterator++;
            }


            connection.Close();
        }



        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadNotesFromDB();   
        }

        private void AddNewNote_Click(object sender, RoutedEventArgs e)
        {
            DateTime? selectedDate = DeadlineData.SelectedDate;

            if (String.IsNullOrEmpty(NoteInput.Text) || selectedDate is null)
                MessageBox.Show("You need to insert the note and deadline");
            else
            {
                if (selectedDate.HasValue && selectedDate > DateTime.Now && NoteInput.Text.Length > 0 && NoteInput.Text.Length < 4000)
                {
                    string ProperFormat = selectedDate!.Value.ToString("yyyy/MM/dd");
                    string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
                    SqlConnection connection = new SqlConnection(connectionString);
                    string SentNote = $"Insert Into Notes(Note, Deadline, UserID) Values('{NoteInput.Text}','{ProperFormat}','{Id}')";

                    connection.Open();
                    SqlCommand InsertNote = new SqlCommand(SentNote, connection);
                    int IsSuccesfull = InsertNote.ExecuteNonQuery();
                    if (IsSuccesfull > 0)
                    {
                        MessageBox.Show("Note Has Been Added Succesfully");
                        // updates the notes right after new one is added
                        LoadNotesFromDB();
                    }
                    else
                    {
                        MessageBox.Show("Note Hasn't been Added");
                    }
                }
                else
                {
                    MessageBox.Show("Wrong input");
                }
            }

            
                
                        
                    

            
        }
        #region Settings
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if(Checkboxes.Visibility == Visibility.Hidden)
                Checkboxes.Visibility = Visibility.Visible;
            else
                Checkboxes.Visibility = Visibility.Hidden;

            
        }

       
        
        private void Highlight_Checked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set DeadlineSetting = '1' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);
        }

        private void Highlight_Unchecked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set DeadlineSetting = '0' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }

        private void Remainder_Checked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set SendRemainder = '1' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }
        private void Remainder_Unchecked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set SendRemainder = '0' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }

        private void Sort_Checked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set SortByDeadline = '1' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);


        }

        private void Sort_Unchecked(object sender, RoutedEventArgs e)
        {
            OpenAndCloseConn((connection) => {
                SqlCommand HighlightChecked = new SqlCommand($"Update Settings Set SortByDeadline = '0' Where UserId = '{Id}'", connection);
                HighlightChecked.ExecuteNonQuery();
            });
            Refresh_Click(sender, e);

        }

        #endregion

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            CurrentNotes.Children.Clear();
            LoadNotesFromDB();
        }

        private void FinBtn_Click(object sender, RoutedEventArgs e)
        {
            // cast the sender parameter to a Button object
            Button? clickedButton = sender as Button;

            // gets the name of specified button so we can get to the exact one 
            // removes the unrequired number
            string NoteID = clickedButton!.Name.Replace('A',' ').Trim();
            OpenAndCloseConn((connection) => { 
                SqlCommand IsFinished = new SqlCommand($"Update Notes Set IsFinished = '1' Where UserId = '{Id}' AND Id = '{NoteID}'",connection);
                IsFinished.ExecuteNonQuery();
            });

            Refresh_Click(sender, e);


        }

        private void FailBtn_Click(object sender, RoutedEventArgs e)
        {
            // cast the sender parameter to a Button object
            Button? clickedButton = sender as Button;

            // gets the name of specified button so we can get to the exact one 
            // removes the unrequired number
            string NoteID = clickedButton!.Name.Replace('A', ' ').Trim();
            OpenAndCloseConn((connection) => {
                SqlCommand IsFailed = new SqlCommand($"Update Notes Set IsFailed = '1' Where UserId = '{Id}' AND Id = '{NoteID}'", connection);
                IsFailed.ExecuteNonQuery();
            });

            Refresh_Click(sender, e);

        }

        private void LoginPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
            
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            History history = new History(Id, Login, Password);
            this.Close();
            history.Show();
        }

        private void Analytics_Click(object sender, RoutedEventArgs e)
        {
            Analytics analytics = new Analytics(Id, Login, Password);
            this.Close();
            analytics.Show();
        }
    }
    }
