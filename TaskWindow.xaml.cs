using System;
using System.Collections.Generic;
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


namespace TaskManagerApp
{
    /// <summary>
    /// Logika interakcji dla klasy TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password;
        public TaskWindow(int id, string login, string password)
        {
            this.Id = id;
            this.Login = login;
            this.Password = password;

            InitializeComponent();
        }

        private void LoadNotesFromDB()
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            SqlCommand ReadNotes = new SqlCommand($"SELECT Note FROM Notes WHERE UserID = '{Id}' AND Deadline > CONVERT(DATETIME, GETDATE(), 101)", connection);
            SqlDataReader readerNotes = ReadNotes.ExecuteReader();




            //SqlCommand countSql = new SqlCommand($"SELECT COUNT(*) AS Note FROM Notes WHERE Id = '4' AND deadline > CONVERT(DATETIME, GETDATE(), 101)");
            //int count = (int)countSql.ExecuteScalar();
            // for record in database that date hasn't expired yet

            // to do rn only one note is visible, repair to make them all show
            int iterator = 0;
            while (readerNotes.Read())
            {

                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Auto);
                CurrentNotes.RowDefinitions.Add(rowDef);

                TextBlock Note = new TextBlock();
                // read value from database
                Note.Text = $"{readerNotes.GetString(0)}";
                Note.Margin = new Thickness(10);

                Note.TextWrapping = TextWrapping.Wrap;
                // creates the frame and assign the properties
                Frame frame = new Frame();
                frame.Content = Note;
                frame.Margin = new Thickness(10);
                frame.BorderThickness = new Thickness(2);
                frame.BorderBrush = Brushes.Black;
                Grid.SetRow(frame, iterator);
                iterator++;


                CurrentNotes.Children.Add(frame);

            }

            /* for (int i = 0; i < count; i++)
             {
                 // Sets the rowDefinition heigth automaticly


             }*/
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
    }
}
