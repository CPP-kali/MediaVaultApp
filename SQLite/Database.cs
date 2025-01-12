using Mysqlx.Cursor;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Windows;


namespace MovieAppMySQL
{
    public class Database
    {
        private readonly SQLiteConnection _database;

        // Constructor para inicializar la conexión a la base de datos SQLite
        public Database()
        {
            string dbPath = "Data Source=MovieApp.sqlite;Version=3;";

            // Crear conexión a SQLite
            _database = new SQLiteConnection(dbPath);

            // Abrir conexión con SQLite
            _database.Open();

            // Crear las tablas si no existen
            CreateTables();
        }

        // Método para crear las tablas si no existen
        private void CreateTables()
        {
            // Crear la tabla User
            string createUserTableQuery = @"
        CREATE TABLE IF NOT EXISTS User (
            Username TEXT PRIMARY KEY,
            Password TEXT NOT NULL
        );";

            // Crear la tabla Item (Películas, Series, Juegos, Libros)
            string createItemTableQuery = @"
            CREATE TABLE IF NOT EXISTS Item (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            PosterUrl TEXT,
            Temporadas TEXT,
            Providers TEXT,
            ElementId INTEGER NOT NULL,
            Rating REAL NOT NULL,  -- RATING DE LA BASE DE DATOS 
            TipoElemento TEXT NOT NULL,  -- Película, Serie, Juego, Libro
            UNIQUE(Id)
        );";

            // Crear la tabla UserItemRating (relación entre usuario e ítem)
            string createUserItemRatingTableQuery = @"CREATE TABLE IF NOT EXISTS UserItem (
            Username TEXT NOT NULL,  -- User's username
            ItemId INTEGER NOT NULL,  -- Reference to the Item table
            Puntuacion REAL NOT NULL,  -- User's rating of the item
            TipoElemento TEXT NOT NULL,  -- Película, Serie, Juego, Libro
            Estado TEXT NOT NULL,  -- 'Consumed' or 'Watchlist' to indicate the user's interaction with the item
            Tags TEXT,  -- Lista de etiquetas separadas por comas
            PRIMARY KEY (Username, ItemId),  -- Composite primary key
            FOREIGN KEY (Username) REFERENCES User (Username) ON DELETE CASCADE,  -- Ensures user exists
            FOREIGN KEY (ItemId) REFERENCES Item (Id) ON DELETE CASCADE  -- Ensures item exists
);";

            // Ejecutar las consultas de creación de tablas
            using (var cmd = new SQLiteCommand(createUserTableQuery, _database))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(createItemTableQuery, _database))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(createUserItemRatingTableQuery, _database))
            {
                cmd.ExecuteNonQuery();
            }
        }




        public void SaveItem(Item item, string estado, double puntuacion)
        {
            try
            {
                using (var transaction = _database.BeginTransaction())
                {
                    string currentUser = CurrentUser.LoggedInUser.Username;


                    string checkItemExistsQuery = @"
                   SELECT COUNT(*) FROM Item 
                    WHERE Title = @Title AND TipoElemento = @TipoElemento;
                     ";

                    using (var checkCommand = new SQLiteCommand(checkItemExistsQuery, _database))
                    {
                        checkCommand.Parameters.AddWithValue("@Title", item.Title);
                        checkCommand.Parameters.AddWithValue("@TipoElemento", item.TipoElemento);

                        long count = (long)checkCommand.ExecuteScalar();

                        if (count == 0)
                        {



                            string insertItemQuery = @"
                              INSERT INTO Item (Title, Description, PosterUrl, Rating, TipoElemento, ElementId, Providers,Temporadas)
                                VALUES (@Title, @Description, @PosterUrl, @Rating, @TipoElemento, @ItemId, @Providers, @Temporadas);
                                 ";

                            using (var insertCommand = new SQLiteCommand(insertItemQuery, _database))
                            {
                                insertCommand.Parameters.AddWithValue("@Title", item.Title);
                                insertCommand.Parameters.AddWithValue("@Description", item.Description);
                                insertCommand.Parameters.AddWithValue("@PosterUrl", item.PosterUrl);
                                insertCommand.Parameters.AddWithValue("@Rating", item.BaseRating);
                                insertCommand.Parameters.AddWithValue("@TipoElemento", item.TipoElemento);
                                insertCommand.Parameters.AddWithValue("@ItemId", item.ItemId);
                                insertCommand.Parameters.AddWithValue("@Providers", item.Providers);
                                insertCommand.Parameters.AddWithValue("@Temporadas", item.Temporadas);
                                insertCommand.ExecuteNonQuery();
                            }


                        }
                    }


                    int elementId = GetItemId(item.Title, item.TipoElemento);

                         string linkUserItemQuery = @"
                            INSERT OR REPLACE INTO UserItem (Username, ItemId, TipoElemento, Estado, Puntuacion, Tags)
                         VALUES (@Username, @ElementId, @TipoElemento, @Estado, @Puntuacion,@Tags);
                           ";

                    using (var linkCommand = new SQLiteCommand(linkUserItemQuery, _database))
                    {
                        linkCommand.Parameters.AddWithValue("@Username", currentUser);
                        linkCommand.Parameters.AddWithValue("@ElementId", elementId);
                        linkCommand.Parameters.AddWithValue("@TipoElemento", item.TipoElemento);
                        linkCommand.Parameters.AddWithValue("@Estado", estado);
                        linkCommand.Parameters.AddWithValue("@Puntuacion", puntuacion);
                        linkCommand.Parameters.AddWithValue("@Tags", "");
                        linkCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                Console.WriteLine("Item saved and linked to user successfully.");
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }



        public int GetItemId(string title, string tipoElemento)
        {
            int Id = -1;  // Default value if item is not found

            // SQL query to retrieve the ElementId based on Title and TipoElemento
            string query = @"
            SELECT Id
            FROM Item
            WHERE Title = @Title AND TipoElemento = @TipoElemento;
            ";

            using (var command = new SQLiteCommand(query, _database))
            {
                // Add parameters to avoid SQL injection
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@TipoElemento", tipoElemento);

                // Execute the query and get the result
                var result = command.ExecuteScalar();
                // Check if a valid result was returned and convert to int
                if (result != null && int.TryParse(result.ToString(), out int id))
                {
                    Id = id;
                }

            }

            return Id;
        }




        // Recuperar todos los ítems guardados por el usuario
        public List<Item> GetItemsForUser(string username)
        {
            List<Item> userItems = new List<Item>();

            string query = @"
        SELECT 
            i.*, 
            ui.Puntuacion, 
            ui.Estado, 
            ui.Tags
        FROM 
            Item i
        INNER JOIN 
            UserItem ui 
            ON i.Id = ui.ItemId 
            AND i.TipoElemento = ui.TipoElemento
        WHERE 
            ui.Username = @Username;
    ";

            using (var command = new SQLiteCommand(query, _database))
            {
                command.Parameters.AddWithValue("@Username", username);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new Item
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ItemId = reader.GetInt32(reader.GetOrdinal("ElementId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            PosterUrl = reader.IsDBNull(reader.GetOrdinal("PosterUrl")) ? null : reader.GetString(reader.GetOrdinal("PosterUrl")),
                            Temporadas = reader.IsDBNull(reader.GetOrdinal("Temporadas")) ? null : reader.GetString(reader.GetOrdinal("Temporadas")),
                            BaseRating = reader.GetDouble(reader.GetOrdinal("Rating")),
                            TipoElemento = reader.GetString(reader.GetOrdinal("TipoElemento")),
                            Puntuacion = reader.GetDouble(reader.GetOrdinal("Puntuacion")),  // From UserItem table
                            Estado = reader.GetString(reader.GetOrdinal("Estado")),
                            Providers = reader.GetString(reader.GetOrdinal("Providers"))// From UserItem table
                            //Tags = reader.IsDBNull(reader.GetOrdinal("Tags")) ? "" : reader.GetString(reader.GetOrdinal("Tags"))  // From UserItem table
                        };

                        userItems.Add(item);
                    }
                }
            }

            return userItems;
        }











        // Crear un nuevo usuario----------------------------------------------------------------------------------
        public void CreateUser(string name, string password, RegistrationWindow registrationWindow)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var existingUser = GetUserByUsername(name);

            if (existingUser == null)
            {
                string query = "INSERT INTO User (Username, Password) VALUES (@Username, @Password)";

                // Using SQLiteCommand for SQLite
                using (var cmd = new SQLiteCommand(query, _database))
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

            // Using SQLiteCommand for SQLite
            using (var cmd = new SQLiteCommand(query, _database))
            {
                cmd.Parameters.AddWithValue("@Username", name);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            Password = reader.GetString(reader.GetOrdinal("Password"))
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
            string deleteMoviesQuery = "DELETE FROM Item WHERE Username = @UserName";
            string deleteUserQuery = "DELETE FROM User WHERE Username = @Username";

            // Using SQLiteCommand for SQLite
            using (var cmd = new SQLiteCommand(deleteMoviesQuery, _database))
            {
                cmd.Parameters.AddWithValue("@UserName", currentuser);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(deleteUserQuery, _database))
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

    // Representación de los ítems (películas, series, juegos, libros) ---------------------------------------------------
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int ItemId { get; set; }
        public string Providers { get; set; }
        public string TipoElemento { get; set; }  // "Movie", "Series", "Game", "Book"
        public double BaseRating { get; set; }  //databas rating
        public double AppRating { get; set; }
        public double Puntuacion { get; set; }
        public string Fecha { get; set; }
        public string Temporadas { get; set; }
        public string Estado { get; set; }




        public Item() { }

        // Parameterized constructor for easier initialization
        public Item(string title, string description, string posterUrl, bool pelic, int movieId, double punts,
               string providers, string userName, double baserating, double apprating, string fecha, string temporadas)
        {
            Title = title;
            Description = description;
            PosterUrl = posterUrl;
            ItemId = movieId;
            Providers = providers;
            Temporadas = temporadas;
            Fecha = fecha;
            BaseRating = baserating;

        }
    }
}
    

