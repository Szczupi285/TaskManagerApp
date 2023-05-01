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





        
        #region register form validation
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

                // Checking if Email or passoword Is already in the database
                SqlCommand CheckIfLoginExists = new SqlCommand(CheckLogin, connection);
                SqlDataReader Login = CheckIfLoginExists.ExecuteReader();
                bool DoesLoginExist = Login.Read();
                Login.Close();

                SqlCommand CheckIfEmailExists = new SqlCommand(CheckEmail, connection);
                SqlDataReader EMail = CheckIfEmailExists.ExecuteReader();
                bool DoesEMailExist = EMail.Read();
                EMail.Close();


                // validation
                if (validateLog && validatePass && validateMail)
                {
                    if(DoesLoginExist == false && DoesEMailExist == false)
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
            // open new window
            TaskWindow window = new TaskWindow();
            window.Show();
            this.Close();
            // else wrong login or password popup
        }

        
    }
}
