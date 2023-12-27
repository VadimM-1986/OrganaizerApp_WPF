using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using WpfAppItProger.Models;

namespace WpfAppItProger
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private AppDBContext _DB;
        public RegisterWindow()
        {
            InitializeComponent();
            _DB = new AppDBContext();
        }

        private void UserRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string email = UserEmail.Text.Trim();
            string password = UserPass.Password.Trim();

            if (login.Equals("") || !email.Contains("@") || password.Length < 3)
            {
                MessageBox.Show("Вы что то не так ввели");
                return;
            }

            User authUser = _DB.Users.Where(el => el.Login == login).FirstOrDefault();
            if (authUser != null)
            {
                MessageBox.Show("Пользователь с таким логинм уже существует!");
                return;
            }

            User user = new User(login, email, Hash(password));
            _DB.Users.Add(user);
            _DB.SaveChanges();

            UserLogin.Text = "";
            UserEmail.Text = "";
            UserPass.Password = "";
            UserRegister.Content = "Готово";
        }

        private string Hash(string input)
        {
            byte[] temp = Encoding.UTF8.GetBytes(input);
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(temp);
                return Convert.ToBase64String(hash);
            }
        }

        private void LinkAuth_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            AuthWindow window = new AuthWindow();
            window.Show();
            Close();
        }
    }
}
