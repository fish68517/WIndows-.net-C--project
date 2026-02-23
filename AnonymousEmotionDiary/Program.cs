using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Initializes the application, runs the main form, and handles cleanup on exit.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Check if running tests
                if (args.Length > 0 && args[0] == "TestRunner")
                {
                    TestRunner.RunTests();
                    return;
                }

                // Application startup initialization
                InitializeApplication();

                // Enable visual styles for modern UI appearance
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Launch main window as application container
                Application.Run(new MainWindow());

                // Application shutdown cleanup
                CleanupApplication();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application initialization failed: {ex.Message}", "Error");
                CleanupApplication();
            }
        }

        /// <summary>
        /// Initializes the application by setting up logging, database, and configuration.
        /// This method is called at application startup.
        /// </summary>
        private static void InitializeApplication()
        {
            try
            {
                // Initialize logging system
                LogInitializer.Initialize();
                LogInitializer.LogDebug("Application startup initiated.");

                // Load configuration
                LogInitializer.LogDebug($"Database path: {ConfigurationHelper.DatabasePath}");
                LogInitializer.LogDebug($"Log file path: {ConfigurationHelper.LogFilePath}");
                LogInitializer.LogDebug($"Emotion high risk threshold: {ConfigurationHelper.EmotionHighRiskThreshold}");

                // Initialize database
                DatabaseManager.Initialize();

                LogInitializer.LogDebug("Application initialization completed successfully.");
            }
            catch (Exception ex)
            {
                LogInitializer.LogError("Application initialization failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Cleans up application resources when the application is shutting down.
        /// This method is called when the application exits.
        /// </summary>
        private static void CleanupApplication()
        {
            try
            {
                LogInitializer.LogDebug("Application shutdown initiated.");

                // Clear user session
                SessionManager.ClearSession();
                LogInitializer.LogDebug("User session cleared.");

                // Close database connection
                DatabaseManager.CloseConnection();
                LogInitializer.LogDebug("Database connection closed.");

                LogInitializer.LogDebug("Application shutdown completed successfully.");
            }
            catch (Exception ex)
            {
                LogInitializer.LogError("Error during application cleanup.", ex);
            }
        }
    }
}
