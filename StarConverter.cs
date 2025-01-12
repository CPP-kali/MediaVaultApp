using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MovieAppMySQL
{
    public class StarConverter: IValueConverter
    {
        // Convert the double rating to a visibility based on the star index
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            double rating = (double)value; // Rating is of type double
            double starIndex = double.Parse(parameter.ToString()); // The star index being checked (1, 2, 3, etc.)

            // Show full star if the rating is >= starIndex
            if (starIndex <= Math.Floor(rating)) // Check if it's a full star
            {
                return Visibility.Visible;
            }

            // Show half star if the rating is >= (starIndex - 0.5) and < starIndex
            // Half stars are visible when the rating is between a whole number and a half
            if (starIndex == 6 && rating % 1 != 0) // For half stars (rating is fractional)
            {
                return Visibility.Visible; // Half star
            }


            if (starIndex == 6 && rating == 0)
            {
                return Visibility.Hidden;

            }
            return Visibility.Collapsed; // Star not visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null; // No need for ConvertBack in this case
        }
    }
}
