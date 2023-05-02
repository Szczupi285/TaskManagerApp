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
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;

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
            
            if (RegisterLogin.Text.Length < 16 && RegisterLogin.Text.Length >3)
            {
                validateLog = true;
                UndBorRegLog.Background = new SolidColorBrush(Color.FromRgb(50,205,50));
                
                LoginPopupText.Visibility = Visibility.Hidden;
            }
            else
            {
                LoginPopupText.Visibility = Visibility.Visible;
                LoginPopupText.Text = "Login length higher than 3 and lower than 16 \u2717";
                validateLog = false;
                UndBorRegLog.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            
        }

     
        private void RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (RegisterPassword.Password.Length < 20 && RegisterPassword.Password.Length > 8 &&
              RegisterPassword.Password.Any(char.IsDigit)
              && RegisterPassword.Password.Any(char.IsUpper))
            {
                validatePass = true;
                UndBorRegPass.Background = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                PasswordPopupText.Visibility = Visibility.Hidden;
            }
            else
            {
                PasswordPopupText.Visibility = Visibility.Visible;
                validatePass = false;
                UndBorRegPass.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }

            StringBuilder s = new StringBuilder();
            if (RegisterPassword.Password.Length > 7 && RegisterPassword.Password.Length < 21)
                s.AppendLine("Password Lenght must be higher than 7 and lower than 21 \u2713");
            else
                s.AppendLine("Password Lenght must be higher than 7 and lower than 21\u2717");
            if (RegisterPassword.Password.Any(char.IsDigit))
                s.AppendLine("Password Must contain Digit \u2713");
            else
                s.AppendLine("Password Must contain Digit \u2717");
            if (RegisterPassword.Password.Any(char.IsUpper))
            {
                s.AppendLine("Password must contain Uppercase letter\u2713");
            }
            else
                s.AppendLine("Password must contain Uppercase letter\u2717");

            PasswordPopupText.Text = s.ToString();


        }
        private void RegisterEMail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RegisterEMail.Text.Length > 6 && RegisterEMail.Text.Length < 60 && RegisterEMail.Text.Contains('@') && RegisterEMail.Text.Contains('.') && RegisterEMail.Text.Substring(RegisterEMail.Text.LastIndexOf('.') + 1).Length >= 2)
            {
                EMailPopupText.Visibility = Visibility.Hidden;
                validateMail = true;
                UndBorRegMail.Background = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            else
            {
                EMailPopupText.Visibility = Visibility.Visible;
                validateMail = false;
                UndBorRegMail.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }

           
                

                StringBuilder s = new StringBuilder();
                if (RegisterEMail.Text.Length < 5 || RegisterEMail.Text.Length > 60)
                    s.AppendLine("Email Lenght must be higher than 5 and lower than 60 \u2717");
                else
                    s.AppendLine("E-Mail Lenght must be higher than 5 and lower than 60 \u2713");
                if ((RegisterEMail.Text.LastIndexOf('@') <= 0))
                    s.AppendLine("E-Mail Must contain @ \u2717");
                else
                    s.AppendLine("E-Mail Must contain @ \u2713");
                if (RegisterEMail.Text.Contains('.') && RegisterEMail.Text.Substring(RegisterEMail.Text.LastIndexOf('.') + 1).Length >= 2)
                {
                    s.AppendLine("E-Mail Must have proper domain \u2713");
                }
                else
                    s.AppendLine("E-Mail Must have proper domain  \u2717");

                EMailPopupText.Text = s.ToString();

                
            
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
