
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;


namespace MovieAppMySQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
        }


        // Evento cuando se hace clic en "Home"
        private void Home_Click(object sender, RoutedEventArgs e)
        {

        }

        // Evento cuando se hace clic en "Search"
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SearchPage());
        }

        // Evento cuando se hace clic en "Watch List"
        private void WatchList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Watched_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Vistas());
        }


        // User List menu item click - Open the User List Window

        
        private void User_Click(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new Usuario());
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click on title bar toggles between Maximized and Normal
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    // Get the current mouse position on screen
                    var mousePosition = PointToScreen(e.GetPosition(this));

                    // Restore the window to Normal state
                    WindowState = WindowState.Normal;

                    // Adjust the window's position so it appears under the mouse
                    Left = mousePosition.X - (RestoreBounds.Width / 2);
                    Top = mousePosition.Y - 20; // Adjust for the title bar height (approximately)

                    // Begin dragging
                    DragMove();
                }
                else
                {
                    // Simply allow dragging in Normal state
                    DragMove();
                }
            }
        }


        

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;  // Minimize the window
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;  // Maximize the window
            else
                this.WindowState = WindowState.Normal;    // Restore the window
        }

        // Close button handler (close the current window)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the current instance of MicroWindow only
            this.Close();
        }




    }
}
