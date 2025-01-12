using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using BCrypt.Net;
using System.Configuration;
using System.Windows;

namespace MovieAppMySQL
{
    public class MovieDatabase
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["MovieAppMySQLConnection"].ConnectionString;

        private readonly MySqlConnection _database;

        // Constructor para inicializar la conexión a la base de datos
        public MovieDatabase()
        {
            string connectionString = ConnectionString; // Actualiza con los detalles de tu conexión MySQL

            // Crear conexión a MySQL
            _database = new MySqlConnection(connectionString);

            // Abrir conexión con MySQL
            _database.Open();

            // Crear las tablas Movie y User si no existen
            CreateTables();
        }

        // Método para crear las tablas si no existen
        private void CreateTables()
        {          
            // Create the User table
            string createUserTableQuery = @"
             CREATE TABLE IF NOT EXISTS User (
            Username VARCHAR(255) PRIMARY KEY,           -- Primary key for User table
            Password VARCHAR(255) NOT NULL               -- User's hashed password
            );
             ";

            // Create the Movie table
            string createMovieTableQuery = @"
            CREATE TABLE IF NOT EXISTS Movie (
            Id INT AUTO_INCREMENT PRIMARY KEY,           -- Auto-increment primary key
            UserName VARCHAR(255) NOT NULL,              -- Foreign key reference to the User table
            Title VARCHAR(255) NOT NULL,                 -- Movie/Series title
            Description TEXT,                            -- Movie/Series description
            PosterUrl VARCHAR(255),                      -- URL for the movie poster
            MovieId INT NOT NULL,                        -- Unique Movie ID from external source
            Peli BOOLEAN NOT NULL,                       -- Boolean flag to distinguish between Movie and TV Series
            Puntuacion DOUBLE NOT NULL,                  -- Movie's user score/rating
            Providers VARCHAR(255),                      -- Providers (e.g., Netflix, Hulu)
            BaseRating DOUBLE,                           -- Base rating (e.g., IMDb rating)
            AppRating DOUBLE,                            -- App-specific rating (e.g., user rating in the app)
            FOREIGN KEY (UserName) REFERENCES User(Username) ON DELETE CASCADE  -- Foreign key to User table
             );
            ";



            // Execute the Movie table creation query
            using (var cmd = new MySqlCommand(createMovieTableQuery, _database))
            {
                cmd.ExecuteNonQuery();
            }

            // Execute the User table creation query
            using (var cmd = new MySqlCommand(createUserTableQuery, _database))
            {
                cmd.ExecuteNonQuery();
            }
        }


        // Método para insertar o actualizar una película---------------------------------------
        public void SaveMovie(Movie movie)
        {
            string currentuser = CurrentUser.LoggedInUser.Username;
            // Asociar la película con el usuario

            // Verificar si una película con el mismo MovieId o Title ya existe para este usuario
            var existingMovie = GetMovieByTitle(movie.Title);

            if (existingMovie != null)
            {
                movie.Id = existingMovie.Id; // Asegurarse de que se actualice la película existente
                UpdateMovie(movie);
            }
            else
            {
                InsertMovie(movie);
            }
        }

        // Insertar una nueva película en la base de datos
        private void InsertMovie(Movie movie)
        {
            string query = @"
                INSERT INTO Movie (UserName, Title, Description, PosterUrl, PosterUrl2, MovieId, Peli, Puntuacion, Providers)
                VALUES (@UserName, @Title, @Description, @PosterUrl, @PosterUrl2, @MovieId, @Peli, @Puntuacion, @Providers);
            ";

            using (var cmd = new MySqlCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Title", movie.Title);
                cmd.Parameters.AddWithValue("@Description", movie.Description);
                cmd.Parameters.AddWithValue("@PosterUrl", movie.PosterUrl);
                cmd.Parameters.AddWithValue("@PosterUrl2", movie.PosterUrl); // Si hay otro póster, se debería agregar PosterUrl2
                cmd.Parameters.AddWithValue("@MovieId", movie.ElementId);
                cmd.Parameters.AddWithValue("@Peli", movie.Peli);
                cmd.Parameters.AddWithValue("@Puntuacion", movie.Puntuacion);
                cmd.Parameters.AddWithValue("@Providers", movie.Providers);

                cmd.ExecuteNonQuery();
            }
        }

        // Actualizar una película existente en la base de datos
        private void UpdateMovie(Movie movie)
        {
            string query = @"
                UPDATE Movie
                SET Title = @Title,
                    Description = @Description,
                    PosterUrl = @PosterUrl,
                    PosterUrl2 = @PosterUrl2,
                    MovieId = @MovieId,
                    Peli = @Peli,
                    Puntuacion = @Puntuacion,
                    Providers = @Providers
                WHERE Id = @Id AND UserName = @UserName;
            ";

            using (var cmd = new MySqlCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Id", movie.Id);
                cmd.Parameters.AddWithValue("@Title", movie.Title);
                cmd.Parameters.AddWithValue("@Description", movie.Description);
                cmd.Parameters.AddWithValue("@PosterUrl", movie.PosterUrl);
                cmd.Parameters.AddWithValue("@PosterUrl2", movie.PosterUrl); // Si hay otro póster, se debería agregar PosterUrl2
                cmd.Parameters.AddWithValue("@MovieId", movie.ElementId);
                cmd.Parameters.AddWithValue("@Peli", movie.Peli);
                cmd.Parameters.AddWithValue("@Puntuacion", movie.Puntuacion);
                cmd.Parameters.AddWithValue("@Providers", movie.Providers);

                cmd.ExecuteNonQuery();
            }
        }

        // Método para recuperar todas las películas guardadas por el usuario
        public List<Movie> GetSavedMovies()
        {
            List<Movie> movies = new List<Movie>();

            try
            {
                if (CurrentUser.LoggedInUser == null || string.IsNullOrEmpty(CurrentUser.LoggedInUser.Username))
                {
                    throw new InvalidOperationException("User is not logged in.");
                }

                string query = "SELECT * FROM Movie WHERE UserName = @UserName";

                if (_database.State != System.Data.ConnectionState.Open)
                {
                    _database.Open();
                }

                using (var cmd = new MySqlCommand(query, _database))
                {
                    cmd.Parameters.AddWithValue("@UserName", CurrentUser.LoggedInUser.Username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var movie = new Movie
                            {
                                
                                Title = reader.GetString("Title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                PosterUrl = reader.IsDBNull(reader.GetOrdinal("PosterUrl")) ? null : reader.GetString("PosterUrl"),
                                ElementId = reader.GetInt32("MovieId"),
                                Peli = reader.GetBoolean("Peli"),
                                Puntuacion = reader.GetDouble("Puntuacion"),
                                Providers = reader.IsDBNull(reader.GetOrdinal("Providers")) ? null : reader.GetString("Providers")
                            };
                            movies.Add(movie);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving movies: {ex.Message}");
                // Opcional: mostrar un mensaje al usuario o registrar el error en un archivo
            }

            return movies;
        }

        // Método para recuperar una película por su título
        public Movie GetMovieByTitle(string title)
        {
            string query = "SELECT * FROM Movie WHERE Title = @Title";
            Movie movie = null;

            using (var cmd = new MySqlCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movie = new Movie
                        {
                            Id = reader.GetInt32("Id"),
                            Title = reader.GetString("Title"),
                            Description = reader.GetString("Description"),
                            PosterUrl = reader.GetString("PosterUrl"),
                            ElementId = reader.GetInt32("MovieId"),
                            Peli = reader.GetBoolean("Peli"),
                            Puntuacion = reader.GetDouble("Puntuacion"),
                            Providers = reader.GetString("Providers")
                        };
                    }
                }
            }
            return movie;
        }

        // Método para eliminar una película por su ID
        public void DeleteMovie(int id)
        {
            string query = "DELETE FROM Movie WHERE Id = @Id AND UserName = @UserName";

            using (var cmd = new MySqlCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserName", CurrentUser.LoggedInUser.Username);
                cmd.ExecuteNonQuery();
            }
        }

        // Crear un nuevo usuario----------------------------------------------------------------------------------
        public void CreateUser(string name, string password, RegistrationWindow registrationWindow)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var existingUser = GetUserByUsername(name);

            if (existingUser == null)
            {
                string query = "INSERT INTO User (Username, Password) VALUES (@Username, @Password)";
                using (var cmd = new MySqlCommand(query, _database))
                {
                    cmd.Parameters.AddWithValue("@Username", name);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"User '{name}' created successfully.");
                CurrentUser.LoggedInUser = new User { Username = name, Password = hashedPassword };

                SaveSession(name);

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                registrationWindow.Close();
            }
            else
            {
                MessageBox.Show("User already exists. Please choose another name.");
            }
        }

        // Comprobar si el nombre de usuario existe
        public User GetUserByUsername(string name)
        {
            string query = "SELECT * FROM User WHERE Username = @Username";
            User user = null;

            using (var cmd = new MySqlCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Username", name);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Username = reader.GetString("Username"),
                            Password = reader.GetString("Password")
                        };
                    }
                }
            }
            return user;
        }
      



// Method to delete a user and their associated movies
        public void DeleteUser()
        {
            string currentuser = CurrentUser.LoggedInUser.Username;
            string deleteMoviesQuery = "DELETE FROM Movie WHERE UserName = @UserName";
            string deleteUserQuery = "DELETE FROM User WHERE Username = @Username";

            using (var cmd = new MySqlCommand(deleteMoviesQuery, _database))
            {
                cmd.Parameters.AddWithValue("@UserName", currentuser);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new MySqlCommand(deleteUserQuery, _database))
            {
                cmd.Parameters.AddWithValue("@Username", currentuser);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine($"User '{currentuser}' and their movies deleted successfully.");
        }

        // Save session (username)
        public void SaveSession(string user)
        {
            string filePath = "session.txt";
            File.WriteAllText(filePath, user);
        }
    }
}



