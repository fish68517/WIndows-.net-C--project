using System;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// Test runner for executing end-to-end tests.
    /// This can be called from the main application or run independently.
    /// </summary>
    public class TestRunner
    {
        /// <summary>
        /// Runs all end-to-end tests.
        /// </summary>
        public static void RunTests()
        {
            try
            {
                // Initialize application infrastructure
                Console.WriteLine("Initializing application infrastructure...\n");
                LogInitializer.Initialize();
                DatabaseManager.Initialize();

                // Run end-to-end tests
                EndToEndTests tests = new EndToEndTests();
                tests.RunAllTests();

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.ReadKey();
            }
        }
    }
}
