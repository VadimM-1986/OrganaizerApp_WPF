using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using WpfAppItProger.Models;
using System.Collections.ObjectModel;

using System.Windows.Media.Animation;
using System.Windows.Media;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace WpfAppItProger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private AppDBContext _DB;
        private const string API_KEY = "cdc49de074c89a4f07684133282c10bb";
        public ObservableCollection<User> Users { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            MainScreen.IsChecked = true;
            SetDefaultSize.IsSelected = true;
            _DB = new AppDBContext();
            LoadUsers();
            userListView.ItemsSource = Users;


            if (!File.Exists("user.xml"))
            {
                ShowAuthWindow();
            }

            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            using (FileStream file = new FileStream("user.xml", FileMode.Open))
            {
                AuthUser auth = (AuthUser) xml.Deserialize(file);
                LogoName.Content = auth.Login;
            }

        }



        // ТАБ ПОГОДА
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            string city = input1.Text.Trim();
            if (city.Length < 2)
            {
                MessageBox.Show("Введите правельный город");
                return;
            }

            try
            {
                string data = await GetWeather(city);
                //обрабатываем json и вытаскиваем определенные данные погоды:
                var json = JObject.Parse(data);
                string temp = json["main"]["temp"].ToString();
                wres.Content = $"В городе {city} температура {temp}°C";
            } catch (HttpRequestException ex)
            {
                MessageBox.Show("Введите правельный город");
                wres.Content = "";
            }
        }

        private async Task<string> GetWeather(string city)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}&units=metric";
            return await client.GetStringAsync(url);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string objName = ((RadioButton)sender).Name;

            StackPanel[] panels = { MainScreePanel, AllUsersPanel, NotesScreePanel, CabinetScreePanel };
            foreach (var pan in panels)
            {
                pan.Visibility = Visibility.Hidden;
            }

            switch(objName)
            {
                case "MainScreen": MainScreePanel.Visibility = Visibility.Visible; break;
                case "AllUsers": AllUsersPanel.Visibility = Visibility.Visible; break;
                case "NotesScreen": NotesScreePanel.Visibility = Visibility.Visible; break;
                case "CabinetScreen": CabinetScreePanel.Visibility = Visibility.Visible; break;
            }
        }



        // ТАБ СПИСОК И УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЕЙ
        private void BtnUserDeleted_Click(object sender, RoutedEventArgs e)
        {
            string login = DeletedLogin.Text.Trim();

            if (login.Equals(""))
            {
                MessageBox.Show("Вы нечего не ввели");
                return;
            }

            using (AppDBContext DBcontext = new AppDBContext())
            {
                // обращаемся к базе данных "DBcontext" и выбираем из таблицы "Users" определенный логин!
                var user = DBcontext.Users.SingleOrDefault(u => u.Login == login);

                // обращаемся к базе данных "DBcontext" и к таблице "Users" и удаляем из таблицы определенную запись!
                if (user != null)
                {
                    DBcontext.Users.Remove(user);
                    DBcontext.SaveChanges();
                    MessageBox.Show($"Пользователь с логином {login} удален");

                    // После удаления пользователя обновляем список пользователей
                    LoadUsers();
                    // Обновляем данные списка в форме
                    userListView.ItemsSource = Users;
                }
                else
                {
                    MessageBox.Show("Такого пользователя не существует!");
                }
                DeletedLogin.Text = "";
            }
        }

        public void LoadUsers()
        {
            using (AppDBContext dbContext = new AppDBContext())
            {
                Users = new ObservableCollection<User>(dbContext.Users.ToList());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            File.Delete("user.xml");
            ShowAuthWindow();
        }

        private void ShowAuthWindow()
        {
            Hide();
            AuthWindow window = new AuthWindow();
            window.Show();
            Close();
        }

        private void ChangeUserBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string email = UserEmail.Text.Trim();

            if (login.Equals("") || !email.Contains("@"))
            {
                MessageBox.Show("Вы что то не так ввели");
                return;
            }

            AppDBContext db = new AppDBContext();
            int countUsers = Convert.ToInt32(db.Users.Count(el => el.Login == login));
            if(countUsers != 0 && !login.Equals(LogoName.Content))
            {
                MessageBox.Show("Пользователь с таким логином уже есть!");
                return;
            }

            User user = db.Users.FirstOrDefault(el => el.Login == LogoName.Content.ToString());

            user.Email = email;
            user.Login = login;

            db.SaveChanges();

            LogoName.Content = login;
            ChangeUserBtn.Content = "Готово";


            AuthUser auth = new AuthUser(login, email);
            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            using (FileStream file = new FileStream("user.xml", FileMode.Create))
            {
                xml.Serialize(file, auth);
            }
        }



        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool isFolder = (bool)openFileDialog.ShowDialog();

            if (isFolder)
            {
                using (Stream stream = File.Open(openFileDialog.FileName, FileMode.Open))
                {
                    using (StreamReader writer = new StreamReader(stream))
                    {
                        UserNotesTextBox.Text = writer.ReadToEnd();
                    }
                }
            }
        }

        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveTextToFile();
        }

        private void TimesNewRomanSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new FontFamily("Times New Roman");
            VerdanaSetText.IsChecked = false;
        }

        private void VerdanaSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new FontFamily("Verdana");
            TimesNewRomanSetText.IsChecked = false;
        }

        private void SelectFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)SelectFontSize.SelectedItem;
            int fontSize = Convert.ToInt32(comboBoxItem.Tag);
            UserNotesTextBox.FontSize = fontSize;
        }

        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (UserNotesTextBox.Text.Trim().Equals(""))
                return;

            SaveTextToFile();
            UserNotesTextBox.Text = "";
        }

        private void SaveTextToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            bool isFolder = (bool)saveFileDialog.ShowDialog();

            if (isFolder)
            {
                using (Stream file = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.Write(UserNotesTextBox.Text);
                    }
                }
            }
        }
    }
}
