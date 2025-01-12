using MySqlX.XDevAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MovieAppMySQL
{
    internal class MovieService
    {

        private static string ApiKey => ConfigurationManager.AppSettings["Api CLAVE TMBD"]; // Reemplaza con tu clave de TMDb
        private const string BaseUrl = "https://api.themoviedb.org/3/";
        //public static bool isMovieSearch = true;    // DICE SI SE BUSCA PELICULA O SERIE 

        // Método para buscar una película
        public static async Task<List<Movie>> SearchMoviesAsync(string query, string region, bool isMovieSearch)
        {
            string searchUrl = isMovieSearch
                ? $"{BaseUrl}search/movie?api_key={ApiKey}&query={Uri.EscapeDataString(query)}&region={region}"
                : $"{BaseUrl}search/tv?api_key={ApiKey}&query={Uri.EscapeDataString(query)}&region={region}"; // Switch to TV search

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    var results = data["results"]?.Take(10); // Limit the results to 10 items

                    if (results != null)
                    {
                        var movies = new List<Movie>();

                        foreach (var movie in results)
                        {
                            // Get basic details
                            string yearRange = "N/A";
                            string temporadas = "N/A";
                            var movieId = movie["id"]?.ToObject<int>() ?? 0;

                            // Fetch detailed info using movie/TV show id
                            var detailedMovieUrl = isMovieSearch
                                ? $"{BaseUrl}movie/{movieId}?api_key={ApiKey}"
                                : $"{BaseUrl}tv/{movieId}?api_key={ApiKey}";

                            HttpResponseMessage detailedResponse = await client.GetAsync(detailedMovieUrl);
                            if (detailedResponse.IsSuccessStatusCode)
                            {
                                string detailedJson = await detailedResponse.Content.ReadAsStringAsync();
                                JObject detailedData = JObject.Parse(detailedJson);

                                if (!isMovieSearch) // For TV shows
                                {
                                    string firstAirDate = detailedData["first_air_date"]?.ToString();
                                    string lastAirDate = detailedData["last_air_date"]?.ToString();

                                    // Set the year range based on the start and end years
                                    int? startYear = !string.IsNullOrWhiteSpace(firstAirDate) ? DateTime.Parse(firstAirDate).Year : (int?)null;
                                    int? endYear = !string.IsNullOrWhiteSpace(lastAirDate) ? DateTime.Parse(lastAirDate).Year : (int?)null;

                                    if (startYear.HasValue && endYear.HasValue && startYear!= endYear)
                                    {
                                        yearRange = $"Emission years: {startYear}–{endYear}";
                                    }
                                    else if (startYear.HasValue)
                                    {
                                        yearRange = $"Emission years: {startYear}";
                                    }

                                    // Fetch the seasons count for TV shows
                                    int seasonsCount = detailedData["seasons"]?.Count() ?? 0;
                                    temporadas = $"Seasons: {seasonsCount}";
                                    
                                }
                                else // For Movies
                                {
                                    yearRange = $"Release Date: {movie["release_date"]?.ToString()}";
                                    int runtime = detailedData["runtime"]?.ToObject<int>() ?? 0;
                                    temporadas = runtime > 0 ? $"Runtime: {runtime} minutes" : "Runtime not available";
                                }

                                var newMovie = new Movie(
                                    movie["title"]?.ToString() ?? movie["name"]?.ToString(), // Movie or TV Show title
                                    movie["overview"]?.ToString(), // Overview
                                    movie["poster_path"] != null
                                        ? $"https://image.tmdb.org/t/p/original{movie["poster_path"]}"
                                        : (movie["backdrop_path"] != null
                                            ? $"https://image.tmdb.org/t/p/original{movie["backdrop_path"]}"
                                            : "https://via.placeholder.com/500"), // Poster or backdrop URL
                                    movie["media_type"]?.ToString() == "movie", // Movie or TV Show
                                    movie["id"]?.ToObject<int>() ?? 0, // Movie ID
                                    0f, // Placeholder for rating (can be updated after fetching more details)
                                    null, // Providers (to be populated later)
                                    null, // Placeholder for username
                                    movie["vote_average"]?.ToObject<double>() ?? 0.0, // Vote average (rating)
                                    0f, // Placeholder for app rating (can be updated)
                                    yearRange,
                                    temporadas
                                );

                                // Fetch providers asynchronously and assign to the movie object
                                string providers = await GetMovieProvidersAsync(newMovie.ElementId, region, isMovieSearch);
                                newMovie.Providers = providers;

                                movies.Add(newMovie);
                            }
                        }

                        return movies;
                    }
                }
            }

            return new List<Movie>(); // Return an empty list if no results found or if the API call fails
        }






        public static async Task<string> GetTrailerUrlAsync(int contentId, bool isMovieSearch, string language)
        {
            string trailerUrl = null;
            string videosUrl = isMovieSearch
                ? $"{BaseUrl}movie/{contentId}/videos?api_key={ApiKey}&language={language}"  // Movie API URL with language
                : $"{BaseUrl}tv/{contentId}/videos?api_key={ApiKey}&language={language}";     // TV Show API URL with language

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(videosUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"JSON Response: {json}");

                        JObject data = JObject.Parse(json);

                        // Look for the first trailer video, if available
                        var results = data["results"];
                        if (results != null && results.HasValues)
                        {
                            // Try to find a trailer first
                            var trailer = results.FirstOrDefault(v => v["type"]?.ToString() == "Trailer");

                            if (trailer != null)
                            {
                                string key = trailer["key"]?.ToString();
                                if (!string.IsNullOrEmpty(key))
                                {
                                    trailerUrl = $"https://www.youtube.com/watch?v={key}"; // Build the YouTube trailer URL
                                    Debug.WriteLine($"Trailer URL found: {trailerUrl}");
                                }
                                else
                                {
                                    Debug.WriteLine("No 'key' property found in the trailer video.");
                                }
                            }
                            else
                            {
                                // No trailer found, try to get any available video
                                var firstVideo = results.FirstOrDefault(); // Get the first available video (not necessarily a trailer)
                                if (firstVideo != null)
                                {
                                    string key = firstVideo["key"]?.ToString();
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        trailerUrl = $"https://www.youtube.com/watch?v={key}"; // Use any available video URL
                                        Debug.WriteLine($"Non-trailer video URL found: {trailerUrl}");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("No 'key' property found in the available video.");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("No video found at all.");
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("'results' is null or empty.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to fetch videos. Status Code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error occurred while fetching trailer URL: {ex.Message}");
                }
            }

            // If no trailer URL is found with the language, fallback to searching without language
            if (string.IsNullOrEmpty(trailerUrl))
            {
                // Retry without language
                string fallbackVideosUrl = isMovieSearch
                    ? $"{BaseUrl}movie/{contentId}/videos?api_key={ApiKey}"  // Movie API URL without language
                    : $"{BaseUrl}tv/{contentId}/videos?api_key={ApiKey}";     // TV Show API URL without language
                using (HttpClient client = new HttpClient())
                    try
                {
                    HttpResponseMessage fallbackResponse = await client.GetAsync(fallbackVideosUrl);
                    if (fallbackResponse.IsSuccessStatusCode)
                    {
                        string fallbackJson = await fallbackResponse.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Fallback JSON Response: {fallbackJson}");

                        JObject fallbackData = JObject.Parse(fallbackJson);

                        var fallbackResults = fallbackData["results"];
                        if (fallbackResults != null && fallbackResults.HasValues)
                        {
                            var fallbackTrailer = fallbackResults.FirstOrDefault(v => v["type"]?.ToString() == "Trailer");

                            if (fallbackTrailer != null)
                            {
                                string key = fallbackTrailer["key"]?.ToString();
                                if (!string.IsNullOrEmpty(key))
                                {
                                    trailerUrl = $"https://www.youtube.com/watch?v={key}"; // Fallback to trailer URL
                                    Debug.WriteLine($"Fallback Trailer URL found: {trailerUrl}");
                                }
                            }
                            else
                            {
                                var fallbackVideo = fallbackResults.FirstOrDefault();
                                if (fallbackVideo != null)
                                {
                                    string key = fallbackVideo["key"]?.ToString();
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        trailerUrl = $"https://www.youtube.com/watch?v={key}"; // Fallback to non-trailer video URL
                                        Debug.WriteLine($"Fallback Non-Trailer Video URL found: {trailerUrl}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to fetch fallback videos. Status Code: {fallbackResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error occurred while fetching fallback trailer URL: {ex.Message}");
                }
            }

            Debug.WriteLine(trailerUrl);
            return trailerUrl; // Return the trailer URL or any other available video URL, or null if none found
        }



        public class AdditionalInfo
        {
            public string DirectorName { get; set; }
            public string ComposerName { get; set; }
            public List<string> Producers { get; set; }
            public List<string> Screenwriters { get; set; }
            public List<string> Showrunners { get; set; } // For TV shows
            public List<string> Countries { get; set; } // For countries
        }


        public static async Task<AdditionalInfo> GetAdditionalInfoAsync(int contentId, bool isMovieSearch)
        {
            var additionalInfo = new AdditionalInfo
            {
                Producers = new List<string>(),
                Screenwriters = new List<string>(),
                Showrunners = new List<string>(),
                Countries = new List<string>() // Initialize countries list
            };

            string creditsUrl = isMovieSearch
                ? $"{BaseUrl}movie/{contentId}/credits?api_key={ApiKey}" // Movie credits endpoint
                : $"{BaseUrl}tv/{contentId}/credits?api_key={ApiKey}";   // TV Show credits endpoint

            string detailsUrl = isMovieSearch
                ? $"{BaseUrl}movie/{contentId}?api_key={ApiKey}"        // Movie details endpoint (to get countries)
                : $"{BaseUrl}tv/{contentId}?api_key={ApiKey}";          // TV Show details endpoint (to get countries)

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Fetch the credits data
                    HttpResponseMessage creditsResponse = await client.GetAsync(creditsUrl);
                    if (creditsResponse.IsSuccessStatusCode)
                    {
                        string creditsJson = await creditsResponse.Content.ReadAsStringAsync();
                        JObject creditsData = JObject.Parse(creditsJson);

                        var crew = creditsData["crew"];
                        if (crew != null && crew.HasValues)
                        {
                            foreach (var member in crew)
                            {
                                string job = member["job"]?.ToString();
                                string name = member["name"]?.ToString();

                                if (!string.IsNullOrEmpty(job) && !string.IsNullOrEmpty(name))
                                {
                                    switch (job)
                                    {
                                        case "Director":
                                            additionalInfo.DirectorName ??= name; // Get the first director found
                                            break;
                                        case "Original Music Composer":
                                            additionalInfo.ComposerName ??= name; // Get the composer
                                            break;
                                        case "Producer":
                                            if (!additionalInfo.Producers.Contains(name))
                                                additionalInfo.Producers.Add(name); // Add producer
                                            break;
                                        case "Screenplay":
                                        case "Writer":
                                            if (!additionalInfo.Screenwriters.Contains(name))
                                                additionalInfo.Screenwriters.Add(name); // Add screenwriter
                                            break;
                                        case "Executive Producer": // Showrunners are usually executive producers
                                            if (!isMovieSearch && !additionalInfo.Showrunners.Contains(name))
                                                additionalInfo.Showrunners.Add(name); // Add showrunner for TV shows
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to fetch credits. Status code: {creditsResponse.StatusCode}");
                    }

                    // Fetch the details to get countries
                    HttpResponseMessage detailsResponse = await client.GetAsync(detailsUrl);
                    if (detailsResponse.IsSuccessStatusCode)
                    {
                        string detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                        JObject detailsData = JObject.Parse(detailsJson);

                        var countries = detailsData["production_countries"];
                        if (countries != null && countries.HasValues)
                        {
                            foreach (var country in countries)
                            {
                                string countryName = country["name"]?.ToString();
                                if (!string.IsNullOrEmpty(countryName) && !additionalInfo.Countries.Contains(countryName))
                                {
                                    additionalInfo.Countries.Add(countryName); // Add country to the list
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to fetch details. Status code: {detailsResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error occurred while fetching credits or countries: {ex.Message}");
                }
            }

            return additionalInfo;
        }





        // Método para obtener las plataformas de streaming
        public static async Task<string> GetMovieProvidersAsync(int movieId, string region, bool isMovieSearch)
        {
            string providersUrl = isMovieSearch
                ? $"{BaseUrl}movie/{movieId}/watch/providers?api_key={ApiKey}"
                : $"{BaseUrl}tv/{movieId}/watch/providers?api_key={ApiKey}"; // For TV shows, not movies

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(providersUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(json);

                        // Access the providers for the specified region
                        var regionProviders = data["results"]?[region];
                        if (regionProviders != null)
                        {
                            var flatrateProviders = regionProviders["flatrate"]?.ToObject<List<JObject>>(); // Extract available platforms
                            if (flatrateProviders != null && flatrateProviders.Any())
                            {
                                // Join the providers' names and return as a string
                                return string.Join(", ", flatrateProviders.Select(p => p["provider_name"]?.ToString()));
                            }
                            else
                            {
                                return "No flat-rate providers available.";
                            }
                        }
                        else
                        {
                            return "No providers found for the specified region.";
                        }
                    }
                    else
                    {
                        return "No data from TMDb.";
                    }
                }
                catch (Exception ex)
                {
                    return $"An error occurred: {ex.Message}";
                }
            }
        }











    }
}

