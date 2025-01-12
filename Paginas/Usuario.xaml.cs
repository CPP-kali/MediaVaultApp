
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace MovieAppMySQL
{
    /// <summary>
    /// Lógica de interacción para Usuario.xaml
    /// </summary>
    public partial class Usuario : Page
    {
        public Usuario()
        {
            InitializeComponent();
        }


        private void EndSession_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "session.txt";  // The file where the user ID is stored

            // Clear the content of the session file to end the session
            File.WriteAllText(filePath, string.Empty);
            CurrentUser.LoggedInUser = null;// This will overwrite the file with an empty string

            MessageBox.Show("Session ended successfully.");



        }













    }
}
