
using System.Windows;


namespace MovieAppMySQL
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        // Handle the Register button click
        public const int Success = 1;
        public const int UserAlreadyExists = 0;
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the values entered by the user
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validate the fields
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.");
                return;
            }


            try
            {
                // Hash the password before saving to the database
               


                // Save the user to the database (assuming App.Database is your database instance)
                App.Database.CreateUser(username, password,this);

                
                // Close the registration windo
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during registration: {ex.Message}");
            }
        }

 
       
    }
}

