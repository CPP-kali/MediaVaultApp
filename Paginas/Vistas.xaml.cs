using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Configuration;

namespace MovieAppMySQL
{
    public partial class Vistas : Page
    {
        public bool IsPosterView;
        private string Estado = "Consumed";

        private string region;
        public string DefaultRegion = "ES";
        private string selectedCategory="Movie";


        public Vistas()
        {
            InitializeComponent();
            CategoryComboBox.SelectedItem = selectedCategory;
            LoadSavedMovies(selectedCategory);
        }

        private async Task LoadSavedMovies(string Categoria)
        {
            try
            {

                // Retrieve saved items (movies, series, games, books)
                List<Item> savedItems = App.Database.GetItemsForUser(CurrentUser.LoggedInUser.Username);

                // Filter the items to only include those where TipoElemento = "Movie"
                List<Item> filteredMovies = savedItems.Where(item => (item.TipoElemento, item.Estado) == (Categoria,Estado)).ToList();

                List<Item> filteredMoviesR= await Get_RegiondependDATA(filteredMovies);

                // Bind the filtered list to the ListBox (SavedMoviesListBox)
                SavedMoviesListBox.ItemsSource = filteredMoviesR;

                if (filteredMovies.Count == 0)
                {
                    MessageBox.Show("No movies found matching the filter.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                MessageBox.Show("Error loading movies: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



        private async Task<List<Item>> Get_RegiondependDATA(List<Item> filteredMovies)
        {
            // Iterar a través de cada película en la lista filteredMovies
            foreach (var selectedMovie in filteredMovies)
            {
                if (selectedMovie != null)
                {
                    try
                    {
                        // Obtener los proveedores de la película utilizando tu MovieService
                        region=region==null? DefaultRegion : region;
                        string providers = selectedCategory=="Movie" || selectedCategory == "TVshow" ? await MovieService.GetMovieProvidersAsync(selectedMovie.ItemId, region, true) 
                            : selectedMovie.Providers;
                        selectedMovie.Providers = $"\n{providers}";
                    }
                    catch (Exception ex)
                    {
                        // Manejar errores de manera adecuada
                        MessageBox.Show("Error al recuperar los proveedores de streaming: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            // Devolver la lista de películas actualizada
            return filteredMovies;
        }



        private void Star_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected movie
            var selectedMovie = SavedMoviesListBox.SelectedItem as Item; // Assuming you're using Item instead of Movie class

            if (selectedMovie != null)
            {
                try
                {
                    // Open the RatingDialog with the current rating
                    RatingDialog ratingDialog = new RatingDialog();

                    // Show the dialog and get the result
                    bool? result = ratingDialog.ShowDialog();

                    if (result == true)
                    {
                        // Update the movie rating
                        // selectedMovie.Puntuacion = ratingDialog.MovieRating;

                        // Save changes to the database using SQLite
                        Database database = new Database();  // Create an instance of your Database class
                        database.SaveItem(selectedMovie, Estado, ratingDialog.MovieRating); // Save or update the item (movie)

                        // Reload the movie list to refresh UI
                        LoadSavedMovies(selectedCategory);
                    }
                }
                catch (Exception ex)
                {
                    // Log error and notify the user
                    Debug.WriteLine($"Error updating movie rating: {ex.Message}");
                    MessageBox.Show("An error occurred while updating the movie rating. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a movie first.", "No Movie Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }




        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get selected item from ComboBox
            var selectedCategor = (sender as ComboBox).SelectedItem as ComboBoxItem;

            // Use selected item's content (e.g., "Movie", "TV Show", or "Game")
            selectedCategory = selectedCategor.Content.ToString();

            LoadSavedMovies(selectedCategory);

        }



        private async void Region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRegion = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (selectedRegion != null)
            {
                region = selectedRegion.Content.ToString();

            }

            List<Item> itemList = SavedMoviesListBox.ItemsSource as List<Item>;
            List <Item> Items = await Get_RegiondependDATA(itemList);
            SavedMoviesListBox.ItemsSource = Items;
            SavedMoviesListBox.Items.Refresh();




        }


    }
}



