using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using WpfAppItProger.Models;

namespace WpfAppItProger
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();

            if (File.Exists("user.xml"))
            {
                ShowMainWindow();
            }


        }

        private void UserAuth_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string password = UserPass.Password.Trim();

            if (login.Equals(""))
            {
                ShakeEffects<TextBox>(UserLogin);
                return;
            }
            else if(password.Length < 3)
            {
                ShakeEffects<PasswordBox>(UserPass);
                return;
            }


            User authUser = null;
            using(AppDBContext dB =  new AppDBContext())
            {
                authUser = dB.Users.Where(user => user.Login == login && user.Password == Hash(password)).FirstOrDefault();
            }

            if (authUser == null)
            {
                MessageBox.Show("Такого пользователя не существует!");
            }
            else
            {
                AuthUser auth = new AuthUser(login, authUser.Email);
                XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
                using (FileStream file = new FileStream("user.xml", FileMode.Create))
                {
                    xml.Serialize(file, auth);
                }

                ShowMainWindow();

            }
        }

        private void ShowMainWindow()
        {
            Hide();
            MainWindow window = new MainWindow();
            window.Show();
            Close();
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

        private void ShakeEffects<Type>(Type widget)
        {
            DoubleAnimation animat = new DoubleAnimation();
            animat.From = 0;
            animat.To = 5;
            animat.Duration = TimeSpan.FromMilliseconds(200);
            animat.RepeatBehavior = new RepeatBehavior(3);
            animat.AutoReverse = true;

            var rotate = new RotateTransform();

            if (widget is TextBox)
                (widget as TextBox).RenderTransform = rotate;
            else if (widget is PasswordBox)
                (widget as PasswordBox).RenderTransform = rotate;
            else
                throw new Exception("Type is not valid");

            rotate.BeginAnimation(RotateTransform.AngleProperty, animat);
        }

        private void LinkRegistr_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            RegisterWindow window = new RegisterWindow();
            window.Show();
            Close();
        }
    }
}
