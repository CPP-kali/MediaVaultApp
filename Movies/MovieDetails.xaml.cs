using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MovieAppMySQL
{
    public partial class MovieDetails : Window
    {
        public MovieDetails()
        {
            InitializeComponent();
        }

        public void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {

                // Start dragging the window (if the click is inside the Border)
                this.DragMove();

            }

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



        public async void SetData(string title, string description, string posterUrl, string trailerUrl, string fechas, string providers, string runtime_temp, 
            string director, string composer, List<string> producer, List<string> writer, List<string> countries)
        {
            MovieTitle.Text = title;
            MovieDescription.Text = description;
            PosterImage.Source = new BitmapImage(new Uri(posterUrl));
            Fecha.Text = fechas;
            Providers.Text = $"Providers: {providers}";
            Temporadas_Runtime.Text = runtime_temp;
            // Correct string interpolation for director and composer
            Director.Text = $"Director: {director}";
            Composer.Text = $"Composer: {composer}";

            // Join the producer and writer lists into comma-separated strings
            Productor.Text = $"Producers: {string.Join(", ", producer)}";
            Writer.Text = $"Writers: {string.Join(", ", writer)}";
            Countries.Text = $"Countries: {string.Join(", ", countries)}";



            if (!string.IsNullOrEmpty(trailerUrl))
            {
                // Asegurarse de que WebView2 esté correctamente inicializado antes de navegar
                await TrailerWebView.EnsureCoreWebView2Async();
                // Navigate to the YouTube trailer URL
                TrailerWebView.CoreWebView2.Navigate(trailerUrl);

            }
            else { trailer.Visibility = Visibility.Collapsed; }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrailerWebView.CoreWebView2 != null)
            { TrailerWebView.CoreWebView2.ExecuteScriptAsync("document.querySelector('video')?.pause();"); }
            this.Close(); // Close the MovieDetails window
        }
    }
}

