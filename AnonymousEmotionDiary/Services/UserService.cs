using System;
using System.Text.RegularExpressions;
using BCrypt.Net;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.DAOs;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Service for managing user authentication and account operations.
    /// Handles user registration, login, and validation logic.
    /// </summary>
    public class UserService
    {
        private readonly UserDAO _userDAO;
        private readonly LogService _logService;

        // Constants for validation
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 20;
        private const int MinPasswordLength = 8;

        /// <summary>
        /// Initializes a new instance of the UserService class.
        /// </summary>
        public UserService()
        {
            _userDAO = new UserDAO();
            _logService = new LogService();
        }

        /// <summary>
        /// Validates the username according to business rules.
        /// Username must be 3-20 characters long and contain only alphanumeric characters and underscores.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username is valid, false otherwise.</returns>
        public bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logService.LogDebug("Username validation failed: username is null or empty");
                return false;
            }

            // Check length
            if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
            {
                _logService.LogDebug($"Username validation failed: length {username.Length} not in range [{MinUsernameLength}, {MaxUsernameLength}]");
                return false;
            }

            // Check for valid characters (alphanumeric and underscore only)
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                _logService.LogDebug("Username validation failed: contains invalid characters");
                return false;
            }

            _logService.LogDebug($"Username validation passed: {username}");
            return true;
        }

        /// <summary>
        /// Validates the password according to business rules.
        /// Password must be at least 8 characters long.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid, false otherwise.</returns>
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logService.LogDebug("Password validation failed: password is null or empty");
                return false;
            }

            // Check minimum length
            if (password.Length < MinPasswordLength)
            {
                _logService.LogDebug($"Password validation failed: length {password.Length} is less than minimum {MinPasswordLength}");
                return false;
            }

            _logService.LogDebug("Password validation passed");
            return true;
        }

        /// <summary>
        /// Registers a new user account with the provided username and password.
        /// Validates input, checks for duplicate username, hashes password, and saves to database.
        /// </summary>
        /// <param name="username">The username for the new account.</param>
        /// <param name="password">The plain text password for the new account.</param>
        /// <returns>The created User object if successful, null otherwise.</returns>
        public User RegisterUser(string username, string password)
        {
            try
            {
                // Validate username
                if (!ValidateUsername(username))
                {
                    _logService.LogDebug($"Registration failed: invalid username format");
                    return null;
                }

                // Validate password
                if (!ValidatePassword(password))
                {
                    _logService.LogDebug($"Registration failed: invalid password format");
                    return null;
                }

                // Check if username already exists
                User existingUser = _userDAO.SelectUserByUsername(username);
                if (existingUser != null)
                {
                    _logService.LogDebug($"Registration failed: username '{username}' already exists");
                    return null;
                }

                // Hash password using BCrypt
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Create new user object
                User newUser = new User(username, passwordHash);

                // Insert into database
                bool insertSuccess = _userDAO.InsertUser(newUser);
                if (!insertSuccess)
                {
                    _logService.LogDebug($"Registration failed: database insertion failed for username '{username}'");
                    return null;
                }

                // Retrieve the created user to get the assigned UserId
                User createdUser = _userDAO.SelectUserByUsername(username);
                if (createdUser != null)
                {
                    _logService.LogDebug($"User registration successful: username '{username}', UserId: {createdUser.UserId}");
                }

                return createdUser;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error during user registration for username '{username}'", ex);
                return null;
            }
        }

        /// <summary>
        /// Authenticates a user with the provided username and password.
        /// Verifies credentials and updates the last login timestamp.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The plain text password to verify.</param>
        /// <returns>The authenticated User object if successful, null otherwise.</returns>
        public User LoginUser(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logService.LogDebug("Login failed: username or password is empty");
                    return null;
                }

                // Retrieve user from database
                User user = _userDAO.SelectUserByUsername(username);
                if (user == null)
                {
                    _logService.LogDebug($"Login failed: user '{username}' not found");
                    return null;
                }

                // Verify password using BCrypt
                bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!passwordMatch)
                {
                    _logService.LogDebug($"Login failed: incorrect password for user '{username}'");
                    return null;
                }

                // Update last login timestamp
                bool updateSuccess = _userDAO.UpdateLastLogin(user.UserId);
                if (!updateSuccess)
                {
                    _logService.LogDebug($"Login warning: failed to update last login time for user '{username}'");
                    // Don't fail the login, just log the warning
                }

                _logService.LogDebug($"User login successful: username '{username}', UserId: {user.UserId}");
                return user;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error during user login for username '{username}'", ex);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a user by their user ID.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>The User object if found, null otherwise.</returns>
        public User GetUserById(int userId)
        {
            try
            {
                User user = _userDAO.SelectUserById(userId);
                if (user != null)
                {
                    _logService.LogDebug($"User retrieved by ID: {userId}");
                }
                else
                {
                    _logService.LogDebug($"User not found for ID: {userId}");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error retrieving user by ID: {userId}", ex);
                return null;
            }
        }
    }
}
