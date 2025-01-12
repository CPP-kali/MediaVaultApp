using System;

namespace MovieAppMySQL
{
    public class Movie
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int ElementId { get; set; }
        public double Puntuacion { get; set; }
        public double BaseRating { get; set; }//databas rating
        public double AppRating { get; set; }
        public string Providers { get; set; }
        public bool Peli { get; set; }  // Indicates if it's a movie or a series
        public string Fecha { get; set; }

        public string Temporadas { get; set; }  //runtime - temporadas
        // Default constructor required by ORM frameworks
        public Movie() { }

        // Parameterized constructor for easier initialization
        public Movie(string title, string description, string posterUrl, bool pelic, int movieId, double punts, 
            string providers, string userName, double baserating, double apprating, string fecha, string temporadas)
        {
            Title = title;
            Description = description;
            PosterUrl = posterUrl;
            ElementId = movieId;
            Puntuacion = punts;
            BaseRating = baserating;
            AppRating = apprating;
            Providers = providers;
            Peli = pelic;
            Fecha = fecha;
            Temporadas = temporadas;
        }
    }
}
