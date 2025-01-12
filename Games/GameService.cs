using Newtonsoft.Json.Linq;
using craftersmine.SteamGridDBNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;

namespace MovieAppMySQL
{
    internal class GameService
    {
        private static string RawgApiKey => ConfigurationManager.AppSettings["Api CLAVE Raw"];
        private const string RawgBaseUrl = "https://api.rawg.io/api/games";
        private static string SteamGridApiKey => ConfigurationManager.AppSettings["Api CLAVE SteamGrid"];

        private static readonly HttpClient _httpClient = new HttpClient();

        // Search for games in RAWG API
        public static async Task<List<Game>> SearchGamesAsync(string query)
        {
            var size = 20;
            string rawgSearchUrl = $"{RawgBaseUrl}?key={RawgApiKey}&page_size={size}&search={Uri.EscapeDataString(query)}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(rawgSearchUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    var results = data["results"]?.Take(size);
                    if (results != null)
                    {
                        var games = new List<Game>();

                        var tasks = results.Select(async game =>
                        {
                            int gameId = game["id"]?.ToObject<int>() ?? 0;
                            string gameTitle = game["name"]?.ToString();
                            var baseRating = game["rating"]?.ToObject<double>() ?? 0.0;
                            string defPosterUrl = game["background_image"]?.ToString() ?? "https://via.placeholder.com/300";

                            // Fetch images concurrently
                            var images = await GetGameImagesFromSteamGrid(gameTitle);

                            // Select the first available image or default to the poster URL
                            string posterUrl = images?.FirstOrDefault() ?? defPosterUrl;

                            // Create and add the game object
                            games.Add(new Game(
                                gameTitle,
                                game["platforms"] != null
                                    ? string.Join(", ", game["platforms"].Select(p => p["platform"]["name"].ToString()))
                                    : "No platforms available",
                                posterUrl,
                                gameId,
                                0f,
                                game["stores"] != null
                                    ? string.Join(", ", game["stores"].Select(p => p["store"]?["name"]?.ToString()))
                                    : "No stores available",
                                null, // UserName
                                baseRating * 2,
                                0f,// AppRating
                                $"Release Date: {game["released"]?.ToString()}",
                                game["publisher"]?.ToString()

                            ));
                        });

                        // Await all game processing tasks
                        await Task.WhenAll(tasks);

                        return games;
                    }
                }

                Debug.WriteLine($"Failed to fetch games from RAWG API. Status Code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching games: {ex.Message}");
            }

            return new List<Game>();
        }



        // Get images from SteamGridDB based on gameTitle
        private static readonly Dictionary<string, List<string>> _imageCache = new Dictionary<string, List<string>>();

        public static async Task<List<string>> GetGameImagesFromSteamGrid(string gameTitle)
        {
            if (_imageCache.ContainsKey(gameTitle))
            {
                return _imageCache[gameTitle]; // Return cached images if available
            }

            try
            {
                var client = new SteamGridDb(SteamGridApiKey);

                var games = await client.SearchForGamesAsync(gameTitle);
                if (games == null || games.Length == 0)
                {
                    return new List<string>();
                }

                var grids = await client.GetGridsByGameIdAsync(games[0].Id);
                if (grids == null || grids.Length == 0)
                {
                    return new List<string>();
                }

                var imageUrls = grids.Select(x => x.FullImageUrl).ToList();

                // Cache the results
                _imageCache[gameTitle] = imageUrls;

                return imageUrls;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching images from SteamGridDB: {ex.Message}");
            }

            return new List<string>();
        }



    }


}

