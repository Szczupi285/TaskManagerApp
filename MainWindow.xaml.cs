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
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Microsoft.Win32;
using System.IO;
using Microsoft.SqlServer.Server;
using System.Security.Cryptography;

namespace TaskManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool validateLog;
        bool validatePass;
        bool validateMail;

        public MainWindow()
        {
            InitializeComponent();

        }

        private static string Hash_SHA256(string password)
        {
            var sha = SHA256.Create();

            var bytes = Encoding.Default.GetBytes(password);

            var passwordHashed = sha.ComputeHash(bytes);

            return Convert.ToBase64String(passwordHashed);

        }
        #region Metods checking if value exist in database
        private static bool DoesLoginExist(string login)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string CheckLogin = $"Select Login FROM Users WHERE Login = '{login}'";
            SqlCommand CheckIfLoginExists = new SqlCommand(CheckLogin, connection);
            SqlDataReader Login = CheckIfLoginExists.ExecuteReader();
            bool DoesLoginExist = Login.Read();

            Login.Close();
            connection.Close();
            return DoesLoginExist;
            
        }

        private static bool DoesPasswordExist(string password)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string CheckPassword = $"Select Password FROM Users WHERE Password = '{password}'";
            SqlCommand CheckIfPasswordExists = new SqlCommand(CheckPassword, connection);
            SqlDataReader Login = CheckIfPasswordExists.ExecuteReader();
            bool DoesPasswordExist = Login.Read();

            Login.Close();
            connection.Close();
            return DoesPasswordExist;
        }

        private static bool DoesEMailExist(string Email)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string CheckEmail = $"Select [E-Mail] From Users WHERE ([E-Mail]) = '{Email}'";
            SqlCommand CheckIfEMailExists = new SqlCommand(CheckEmail, connection);
            SqlDataReader EMail = CheckIfEMailExists.ExecuteReader();
            bool DoesPasswordExist = EMail.Read();

            EMail.Close();
            connection.Close();
            return DoesPasswordExist;
        }

        #endregion




        #region register form validation realtime
        private void RegisterLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (RegisterLogin.Text.Length < 16)
            {
                validateLog = true;
                UndBorRegLog.Background = new SolidColorBrush(Color.FromRgb(50,205,50));
            }
            else
            {
                validateLog = false;
                UndBorRegLog.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

     
        private void RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (RegisterPassword.Password.Length < 20 && RegisterPassword.Password.Length > 8 &&
              RegisterPassword.Password.Any(char.IsDigit)
              && RegisterPassword.Password.Any(char.IsLetter) && RegisterPassword.Password.Any(char.IsUpper))
            {
                validatePass = true;
                UndBorRegPass.Background = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            else
            {
                validatePass = false;
                UndBorRegPass.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }

        private void RegisterEMail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RegisterEMail.Text.Length < 60 && RegisterEMail.Text.Contains('@') && RegisterEMail.Text.Contains('.') && RegisterEMail.Text.IndexOf('.') < RegisterEMail.Text.Length -2)
            {
                validateMail = true;
                UndBorRegMail.Background = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            else
            {
                validateMail = false;
                UndBorRegMail.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
        }
        #endregion

        private void RegisterSend_Click(object sender, RoutedEventArgs e)
        {
            // hash password
            // take to constider what path you will be using so it will work on every pc
            try
            {

                
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\repository\TaskManagerApp\Database1.mdf;Integrated Security=True";
                SqlConnection connection = new SqlConnection(connectionString);

                string HashedPassword = Hash_SHA256(RegisterPassword.Password);
                string insertValues = $"INSERT INTO Users (Login, password, [E-mail]) Values('{RegisterLogin.Text}', '{HashedPassword}', '{RegisterEMail.Text}')";
                string CheckLogin = $"Select Login FROM Users WHERE Login = '{RegisterLogin.Text}'";
                string CheckEmail = $"Select [E-Mail] From Users WHERE ([E-Mail]) = '{RegisterEMail.Text}'";

                connection.Open();

                // Checking if login or Email Is already in the database
                bool DoesLogExist = DoesLoginExist(RegisterLogin.Text);
                bool DoesEmExist = DoesEMailExist(RegisterEMail.Text);
               


                // validation
                if (validateLog && validatePass && validateMail)
                {
                    if(DoesLogExist == false && DoesEmExist == false)
                    {
                        SqlCommand command = new SqlCommand(insertValues, connection);
                        int IsSuccesfull = command.ExecuteNonQuery();
                        if (IsSuccesfull > 0)
                        {
                            MessageBox.Show("You have registered successfully");
                        }
                        else
                        {
                            MessageBox.Show("Registration process failed");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Login or E-Mail is already used");
                    }
                    
                }
                
                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show($"Something went wrong");
            }

        }

        private void LoginSend_Click(object sender, RoutedEventArgs e)
        {
            // if(login = databaseLogin && password = databasePassoword)
            // open TaskWindow
            bool DoesLogExist = DoesLoginExist(LoginLogin.Text);
            string HashedPassword = Hash_SHA256(LoginPassword.Password);
            bool DoesPassExist = DoesPasswordExist(HashedPassword);
            if (DoesLogExist && DoesPassExist)
            {
                TaskWindow window = new TaskWindow();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Wrong login or password");
            }
            
        }

        
    }
}
