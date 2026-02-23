using System;
using AnonymousEmotionDiary.Models;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// Manages the current user session for the application.
    /// Stores and provides access to the currently logged-in user information.
    /// </summary>
    public static class SessionManager
    {
        private static User _currentUser;

        /// <summary>
        /// Gets the currently logged-in user.
        /// </summary>
        public static User CurrentUser
        {
            get { return _currentUser; }
        }

        /// <summary>
        /// Gets a value indicating whether a user is currently logged in.
        /// </summary>
        public static bool IsLoggedIn
        {
            get { return _currentUser != null; }
        }

        /// <summary>
        /// Sets the current user session.
        /// </summary>
        /// <param name="user">The user to set as the current session user.</param>
        public static void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        /// <summary>
        /// Clears the current user session.
        /// </summary>
        public static void ClearSession()
        {
            _currentUser = null;
        }

        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        /// <returns>The current user's ID, or -1 if no user is logged in.</returns>
        public static int GetCurrentUserId()
        {
            return _currentUser?.UserId ?? -1;
        }

        /// <summary>
        /// Gets the current user's username.
        /// </summary>
        /// <returns>The current user's username, or null if no user is logged in.</returns>
        public static string GetCurrentUsername()
        {
            return _currentUser?.Username;
        }
    }
}
