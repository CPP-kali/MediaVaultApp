using System;

namespace MovieAppMySQL
{
    public class User
    {
        // Primary Key (Auto Increment) in MySQL will automatically be handled in the database schema

        public string Username { get; set; }

        public string Password { get; set; } // Store hashed passwords for security in a real application
    }
}

