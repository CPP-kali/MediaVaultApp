
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace MovieAppMySQL
{
    /// <summary>
    /// Lógica de interacción para SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public SearchPage()
        {
            InitializeComponent();
            
        }

        
        private bool isTvShowSearch = false;
        private bool isGameSearch = false;
        private bool isMovieSearch = false;
        private bool isBookSearch = false;
        private string region;
        public string DefaultRegion = "ES";



        private async void BuscaGame()
        {
            string gameName = MovieNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(gameName))
            {
                try
                {
                    var games = await GameService.SearchGamesAsync(gameName);

                    if (games.Any())
                    {


                        MoviesListBox.ItemsSource = games;
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                        MoviesListBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("No Games found.");
                        MoviesListBox.ItemsSource = null;
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingresa el nombre de una película.");
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }


        private async void BuscaBook()
        {
            string bookName = MovieNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(bookName))
            {
                try
                {
                    var books = await Bookservice.SearchBooksAsync(bookName);

                    if (books.Any())
                    {


                        MoviesListBox.ItemsSource = books;
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                        MoviesListBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("No Games found.");
                        MoviesListBox.ItemsSource = null;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingresa el nombre de una película.");
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }



        private async void BuscaPelicula()
        { 
            if (region==null) { region = DefaultRegion; }
            string movieName = MovieNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(movieName))
            {
                try
                {
                   
                    var movies = await MovieService.SearchMoviesAsync(movieName, region, isMovieSearch);

                    if (movies.Any())
                    {


                        MoviesListBox.ItemsSource = movies;
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                        MoviesListBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("No movies found.");
                        MoviesListBox.ItemsSource = null;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingresa el nombre de una película.");
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }





        // BUSQUEDA que se activa cuando se presiona una tecla en el TextBox
        private void MovieNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica si la tecla presionada es Enter
            if (e.Key == Key.Enter)
            {
                
                // Llama a la función que quieres activar al presionar Enter
                MoviesListBox.Visibility = Visibility.Collapsed;
                LoadingProgressBar.Visibility = Visibility.Visible;
                if (isTvShowSearch || isMovieSearch) { BuscaPelicula(); }
                else if (isGameSearch) { BuscaGame(); }
                else if (isBookSearch) { BuscaBook(); }
                else { LoadingProgressBar.Visibility = Visibility.Collapsed; }
            }
        }




        //FUNCION PARA REHACER BUSQUEDA
        private  void UpdateSearch()
        {
            
            if (!string.IsNullOrEmpty(MovieNameTextBox.Text))
            {
               MoviesListBox.Visibility = Visibility.Collapsed;
               LoadingProgressBar.Visibility = Visibility.Visible;
               if (isTvShowSearch || isMovieSearch) { BuscaPelicula(); }
               else if (isGameSearch) { BuscaGame(); }
               else if (isBookSearch) {BuscaBook(); }
               // Refresh the MoviesListBox to reflect the updated data
               MoviesListBox.Items.Refresh();
            }
            
            

 
        }


        private void AddToConsumed_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;



            dynamic elementoSel = null;

            if (isMovieSearch || isTvShowSearch)
            {
                 elementoSel = button?.DataContext as Movie;
            }
            else if (isGameSearch)
            {
                 elementoSel = button?.DataContext as Game;
            }
            else if (isBookSearch)
            {
                 elementoSel = button?.DataContext as Book;
            }


            var TipoElemento = isMovieSearch ? "Movie" : isTvShowSearch ? "TVshow" : isGameSearch ? "Game" : isBookSearch ? "Book" : null;

            if (elementoSel != null)
            {
                try
                {

                    Item item = new Item();
                    // Check if the movie already exists
                    var existingItems = App.Database.GetItemsForUser(CurrentUser.LoggedInUser.Username);
                    if (existingItems.Any(m => (m.Title, m.TipoElemento) == (elementoSel.Title, TipoElemento)))
                    {
                        MessageBox.Show("Este elemento ya está guardado.");
                        return;
                    }


                    // Show the RatingDialog to get the user's rating
                    RatingDialog ratingDialog = new RatingDialog();
                    bool? result = ratingDialog.ShowDialog(); // Show the dialog modally

                    // If the user clicked "Confirm"
                    if (result == true)
                    {
                        // Get the rating from the dialog
                        item.Title = elementoSel.Title;
                        item.PosterUrl = elementoSel.PosterUrl;
                        item.Providers = elementoSel.Providers;
                        item.Description = elementoSel.Description;
                        item.Temporadas = elementoSel.Temporadas;
                        item.TipoElemento = TipoElemento;
                        item.PosterUrl = elementoSel.PosterUrl;
                        item.ItemId = elementoSel.ElementId ?? 12;
                        item.BaseRating = elementoSel.BaseRating;
                        item.Providers = elementoSel.Providers; // Save the movie with the rating in the database
                        App.Database.SaveItem(item, "Consumed", ratingDialog.MovieRating);

                        MessageBox.Show("Película guardada con éxito.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la película: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No se pudo guardar la película.");
            }
        }



        private void AddToWatchList_Click(object sender, RoutedEventArgs e)
        {

        }

    
        







     




  






        // Event handler for when a checkbox is checked/unchecked

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get selected item from ComboBox
            var selectedCategor = (sender as ComboBox).SelectedItem as ComboBoxItem;

            // Use selected item's content (e.g., "Movie", "TV Show", or "Game")
            string selectedCategory = selectedCategor.Content.ToString();

            // Your logic based on selected category
            if (selectedCategory == "Movie")
            {
                isMovieSearch = true;
                isGameSearch = false;
                isTvShowSearch = false;
            }
            else if (selectedCategory == "TV Show")
            {
                isMovieSearch= false;
                isTvShowSearch = true;
                isGameSearch=false;
            }
            else if (selectedCategory == "Game")
            {
                isGameSearch = true;
                isMovieSearch = false;
                isTvShowSearch = false;
            }
            else if (selectedCategory == "Book")
            {
                isBookSearch = true;
                isMovieSearch = false;
                isTvShowSearch = false;
                isGameSearch = false;
            }



                UpdateSearch();
        }
        


        private void Region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRegion = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (selectedRegion != null)
            {
                region = selectedRegion.Content.ToString();
               
            }

            UpdateSearch();
        }


        public static string GetLanguageCodeFromRegion(string region)
        {
            // Mapping of region code to language code
            var regionToLanguageMap = new Dictionary<string, string>
    {
        { "US", "en-US" },
        { "ES", "es-ES" },
        { "FR", "fr-FR" },
        { "DE", "de-DE" },
        { "IT", "it-IT" },
        { "BR", "pt-BR" },
        { "MX", "es-MX" },
        { "JP", "ja-JP" },
        { "IN", "hi-IN" },
        { "GB", "en-GB" },
        { "CA", "fr-CA" },
        { "AU", "en-AU" },
        { "RU", "ru-RU" },
        // Add other mappings as necessary
    };

            // Try to find the language code for the given region
            if (regionToLanguageMap.ContainsKey(region))
            {
                return regionToLanguageMap[region];
            }

            // If region is not found, return a default language (en-US)
            return "en-US";
        }

        private MovieDetails microWindow;

        private async void MoviesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (microWindow == null) // Prevent multiple instances
            {
                if (sender is ListBox listBox && listBox.SelectedItem is Movie selectedItem)
                {
                    try
                    {
                        // Disable main window interaction and show a loading indicator
                        Application.Current.MainWindow.IsEnabled = false; // Disable interaction with the main window

                        // Fetch trailer URL and additional information asynchronously
                        var trailer = await MovieService.GetTrailerUrlAsync(selectedItem.ElementId, isMovieSearch, GetLanguageCodeFromRegion(region));
                        var additionalInfo = await MovieService.GetAdditionalInfoAsync(selectedItem.ElementId, isMovieSearch);

                        // Create and configure the MicroWindow
                        microWindow = new MovieDetails();
                        microWindow.SetData(selectedItem.Title, selectedItem.Description, selectedItem.PosterUrl, trailer, selectedItem.Fecha, selectedItem.Providers, selectedItem.Temporadas,
                            additionalInfo.DirectorName, additionalInfo.ComposerName, additionalInfo.Producers, additionalInfo.Screenwriters, additionalInfo.Countries);

                        // Ensure window ownership and input focus
                        microWindow.Owner = Application.Current.MainWindow; // Set owner
                        microWindow.Closed += (s, args) => {
                            microWindow = null;
                            Application.Current.MainWindow.IsEnabled = true; // Re-enable main window when the micro window closes
                            // Hide loading indicator
                        };

                        // Show the MicroWindow
                        microWindow.Show();
                        microWindow.Activate();
                        microWindow.Focus(); // Bring to focus

                        Debug.WriteLine("MicroWindow is opened and ready for input.");
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors that occur during loading
                        MessageBox.Show($"An error occurred while opening the movie details: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("MicroWindow is already open.");
            }

            Application.Current.MainWindow.IsEnabled = true;
        }

      



        private void PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check if MicroWindow is open and if the click is outside the MicroWindow
            if (microWindow != null)
            {
                if (microWindow.TrailerWebView.CoreWebView2 != null)
                { microWindow.TrailerWebView.CoreWebView2.ExecuteScriptAsync("document.querySelector('video')?.pause();"); }
            
                microWindow.Close();
                microWindow = null;
            }
                // Close the MicroWindow if the user clicks outside
           
            
        }











    }
}
