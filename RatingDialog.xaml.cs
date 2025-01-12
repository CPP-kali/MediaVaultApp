using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieAppMySQL
{
    /// <summary>
    /// Lógica de interacción para RatingDialog.xaml
    /// </summary>
    public partial class RatingDialog : Window
    {
        public float MovieRating { get; private set; }

        public RatingDialog()
        {
            InitializeComponent();
            MovieRating = 0; // Default rating
        }
        // Event handler for the Slider's ValueChanged event
        private void RatingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the TextBlock with the current value of the Slider
            RatingValueTextBlock.Text = RatingSlider.Value.ToString("0.0"); // Format as a single decimal
        }

        // When the user clicks "Confirm", we set the rating and close the dialog
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            MovieRating = (float)RatingSlider.Value; // Get the selected rating
            this.DialogResult = true; // Indicate the dialog was confirmed
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Indicate the dialog was canceled
            this.Close(); // Close the dialog
        }
    }
}

