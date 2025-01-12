using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MovieAppMySQL
{
    internal class Bookservice
    {
        private const string GoogleBooksBaseUrl = "https://www.googleapis.com/books/v1/";
        private static string ApiKey => ConfigurationManager.AppSettings["GoogleBooksApiKey"]; // Reemplazar con tu API Key

        // Método para buscar libros
        public static async Task<List<Book>> SearchBooksAsync(string query, int maxResults = 10)
        {
            string searchUrl = $"{GoogleBooksBaseUrl}volumes?q={Uri.EscapeDataString(query)}&maxResults={maxResults}&key={ApiKey}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    var results = data["items"];
                    if (results != null)
                    {
                        var books = new List<Book>();
                        foreach (var item in results)
                        {
                            var volumeInfo = item["volumeInfo"];
                            if (volumeInfo == null) continue;

                            int numvotos = volumeInfo["ratingsCount"]?.ToObject<int?>() ?? 0;

                            // Intentar extraer el ISBN-13
                            var industryIdentifiers = volumeInfo["industryIdentifiers"]?.ToArray();
                            string isbn = industryIdentifiers?
                                .FirstOrDefault(id => id["type"]?.ToString() == "ISBN_13")?["identifier"]?.ToString();

                            // Try to convert the 'id' from string to an integer
                            int elementId = 0;
                            int.TryParse(item["id"]?.ToString(), out elementId); // If conversi

                            var book = new Book
                            {
                                Title = volumeInfo["title"]?.ToString(),
                                Authors = volumeInfo["authors"]?.ToObject<List<string>>() ?? new List<string> { "Unknown Author" },
                                Description = volumeInfo["description"]?.ToString() ?? "No description available.",

                                // Intentamos con Open Library primero, agotando todos los tamaños disponibles
                                PosterUrl = TryGetOpenLibraryCover(isbn)
                                            ?? TryGetGoogleBooksCover(volumeInfo)
                                            ?? "https://via.placeholder.com/128x190",

                                Fecha = volumeInfo["publishedDate"]?.ToString(),
                                Providers = $"Categories: {string.Join(", ", volumeInfo["categories"]?.ToObject<List<string>>() ?? new List<string>())}",
                                BaseRating = volumeInfo["averageRating"]?.ToObject<double?>() ?? 0,
                                Temporadas = $"Number of Votes: {numvotos}",
                                PreviewLink = volumeInfo["previewLink"]?.ToString(),
                                ElementId = elementId
                            };

                            books.Add(book);
                        }
                        return books;
                    }
                }
            }

            return new List<Book>();
        }

        // Método auxiliar para Open Library Cover URL en diferentes tamaños
        private static string TryGetOpenLibraryCover(string isbn)
        {
            if (string.IsNullOrEmpty(isbn)) return null;

            // Probar con diferentes tamaños: S, M, L, XL
            string[] sizes = {"M", "L", "XL" };
            foreach (var size in sizes)
            {
                string coverUrl = GetOpenLibraryCoverUrl(isbn, size);
                if (!string.IsNullOrEmpty(coverUrl))
                {
                    return coverUrl;
                }
            }

            return null;  // Si no hay portada
        }

        // Obtener la portada desde Open Library por ISBN y tamaño
        private static string GetOpenLibraryCoverUrl(string isbn, string size = "L")
        {
            return $"https://covers.openlibrary.org/b/isbn/{isbn}-{size}.jpg";
        }

        // Método auxiliar para obtener la portada desde Google Books
        private static string TryGetGoogleBooksCover(JToken volumeInfo)
        {
            // Intentar obtener la portada de Google Books (tamaño "large")
            var imageLink = volumeInfo["imageLinks"]?["large"]?.ToString();
            if (!string.IsNullOrEmpty(imageLink))
            {
                return imageLink;
            }

            // Si no hay en "large", intentamos con el tamaño "medium" o "thumbnail"
            imageLink = volumeInfo["imageLinks"]?["medium"]?.ToString() ?? volumeInfo["imageLinks"]?["small"]?.ToString() ?? volumeInfo["imageLinks"]?["thumbnail"]?.ToString();
            return imageLink;
        }
    }
}
