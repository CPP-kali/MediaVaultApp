using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAppMySQL
{
    public class Book
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string Fecha { get; set; }
        public string Providers { get; set; } //categories
        public double? BaseRating { get; set; }
        public string Temporadas { get; set; }  // ratingCount
        public string PreviewLink { get; set; }
        public int ElementId { get; set; }
    }
}
