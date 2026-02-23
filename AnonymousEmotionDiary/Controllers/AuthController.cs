using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// Controller for handling user authentication operations.
    /// Manages user registration, login, and logout flows.
    /// </summary>
    public class AuthController
    {
        private readonly UserService _userService;
        private readonly LogService _logService;

        /// <summary>
        /// Initializes a new instance of the AuthController class.
        /// </summary>
        public AuthController()
        {
            _userService = new UserService();
            _logService = new LogService();
        }

        /// <summary>
        /// Handles user registration.
        /// Validates input, calls UserService to register, and displays appropriate messages.
        /// </summary>
        /// <param name="username">The username for the new account.</param>
        /// <param name="password">The password for the new account.</param>
        /// <param name="confirmPassword">The password confirmation.</param>
        /// <returns>True if registration was successful, false otherwise.</returns>
        public bool HandleRegister(string username, string password, string confirmPassword)
        {
            try
            {
                // Validate input is not empty
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    _logService.LogDebug("Registration failed: empty input fields");
                    MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Validate username format
                if (!_userService.ValidateUsername(username))
                {
                    _logService.LogDebug($"Registration failed: invalid username format for '{username}'");
                    MessageBox.Show("Username must be 3-20 characters long and contain only alphanumeric characters and underscores.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Validate password format
                if (!_userService.ValidatePassword(password))
                {
                    _logService.LogDebug("Registration failed: invalid password format");
                    MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Validate passwords match
                if (password != confirmPassword)
                {
                    _logService.LogDebug("Registration failed: passwords do not match");
                    MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Attempt registration
                User newUser = _userService.RegisterUser(username, password);

                if (newUser != null)
                {
                    _logService.LogDebug($"Registration successful for user: {username}");
                    MessageBox.Show($"Registration successful! Welcome, {newUser.Username}. You can now login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    _logService.LogDebug($"Registration failed: UserService returned null for username '{username}'");
                    MessageBox.Show("Registration failed. Username may already exist or an error occurred.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception during registration for username '{username}'", ex);
                MessageBox.Show("An unexpected error occurred during registration. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Handles user login.
        /// Validates input, calls UserService to authenticate, and saves session if successful.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>The authenticated User object if successful, null otherwise.</returns>
        public User HandleLogin(string username, string password)
        {
            try
            {
                // Validate input is not empty
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logService.LogDebug("Login failed: empty username or password");
                    MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Attempt login
                User authenticatedUser = _userService.LoginUser(username, password);

                if (authenticatedUser != null)
                {
                    // Save user session
                    SessionManager.SetCurrentUser(authenticatedUser);
                    _logService.LogDebug($"Login successful for user: {username}, session saved");
                    return authenticatedUser;
                }
                else
                {
                    _logService.LogDebug($"Login failed: invalid credentials for username '{username}'");
                    MessageBox.Show("Invalid username or password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception during login for username '{username}'", ex);
                MessageBox.Show("An unexpected error occurred during login. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Handles user logout.
        /// Clears the user session and returns to the login interface.
        /// </summary>
        /// <returns>True if logout was successful, false otherwise.</returns>
        public bool HandleLogout()
        {
            try
            {
                string currentUsername = SessionManager.GetCurrentUsername();
                
                // Clear session
                SessionManager.ClearSession();
                _logService.LogDebug($"User logout successful: {currentUsername}");

                // Show logout message
                MessageBox.Show("You have been logged out successfully.", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Exception during logout", ex);
                MessageBox.Show("An error occurred during logout.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
