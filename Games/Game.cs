using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAppMySQL
{
    public class Game
    {
        public int Id { get; set; }  // Primary Key (auto-incremented in MySQL)
        public string Title { get; set; }
        public string Description { get; set; } // PLATFORMS
             
        public string PosterUrl { get; set; }

        public int ElementId { get; set; }

        public double Puntuacion { get; set; }

        public double BaseRating { get; set; }//METACRITIC

        public double AppRating { get; set; } 

        public string Providers { get; set; } // tiendas 

        public string Fecha { get; set; }

        public string Temporadas { get; set; }   //genres

        public Game() { }

        public Game(string title, string description, string posterUrl, int gameId, double punts,
            string providers, string userName, double baserating, double apprating, string fecha, string temporadas)
        {
            Title = title;
            Description = description;
            PosterUrl = posterUrl;
            ElementId = gameId;
            Puntuacion = punts;
            BaseRating = baserating;
            AppRating = apprating;
            Providers = providers;
            Fecha = fecha;
            Temporadas = temporadas;

        }
    }
}


