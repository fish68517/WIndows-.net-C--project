using System;

namespace AnonymousEmotionDiary.Models
{
    /// <summary>
    /// Represents a user account in the system.
    /// Contains user authentication information and account metadata.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username for the user account.
        /// Must be unique and between 3-20 characters.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the hashed password for the user account.
        /// Passwords are hashed using BCrypt for security.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the user's last login.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the User class.
        /// </summary>
        public User()
        {
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the User class with specified parameters.
        /// </summary>
        /// <param name="username">The username for the account.</param>
        /// <param name="passwordHash">The hashed password.</param>
        public User(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the user.
        /// </summary>
        /// <returns>A formatted string containing user information.</returns>
        public override string ToString()
        {
            return $"User: {Username} (ID: {UserId}, Created: {CreatedAt:yyyy-MM-dd HH:mm:ss})";
        }
    }
}
