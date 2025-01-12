using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace MovieAppMySQL
{
    /// <summary>
    /// Lógica de interacción para StartSession.xaml
    /// </summary>
    public partial class StartSession : Window
    {
        public StartSession()
        {
            InitializeComponent();
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var user = App.Database.GetUserByUsername(username);  // Método para obtener el usuario desde la base de datos
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))  // Verificar contraseña
            {
                // Si la contraseña es correcta, almacenamos el usuario como el usuario activo
                CurrentUser.LoggedInUser = user;
                SaveSession(user.Username);

                // Abrimos la ventana principal
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Cerramos la ventana de login
                this.Close();
            }
            else if(user == null)
            {
                MessageBox.Show("Invalid username ");
            }
            else
            {
                MessageBox.Show("Invalid  password.");
            }
        
        }



        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registerwindow = new RegistrationWindow();
            this.Close();// Assuming MainWindow is your main application window
            registerwindow.Show();
        }

        public void SaveSession(string user)
        {
            string filePath = "session.txt";  // El archivo donde se guarda el ID de usuario

            // Guardar el ID del usuario en el archivo
            File.WriteAllText(filePath, user.ToString());
        }



    }
}
