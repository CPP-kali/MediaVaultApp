
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MovieAppMySQL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        // Database : SQLite
        public static Database Database { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Database = new Database();


            CheckSession();



            if (CurrentUser.LoggedInUser != null)
            {
                MainWindow mainWindow = new MainWindow(); // Assuming MainWindow is your main application window
                mainWindow.Show();

            }
            else
            {
                StartSession inicioSesion = new StartSession();
                inicioSesion.Show();
            }





        }


        public void CheckSession()
        {
            string filePath = "session.txt";  // El archivo donde guardamos la sesión

            if (File.Exists(filePath))
            {
                // Leer el ID del usuario desde el archivo
                string userName = (File.ReadAllText(filePath));

                // Recuperar al usuario desde la base de datos
                var user = App.Database.GetUserByUsername(userName);
                if (user != null)
                {
                    CurrentUser.LoggedInUser = user;
                }
            }
        }




      




        public App()
        {

        }


    }

}
