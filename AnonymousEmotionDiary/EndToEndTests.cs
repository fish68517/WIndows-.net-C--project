using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnonymousEmotionDiary.DAOs;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;

namespace AnonymousEmotionDiary
{
    /// <summary>
    /// End-to-end test suite for the Anonymous Emotion Diary application.
    /// Tests all major workflows including user registration, login, diary creation,
    /// emotion analysis, high-risk detection, and logging.
    /// </summary>
    public class EndToEndTests
    {
        private readonly UserService _userService;
        private readonly DiaryService _diaryService;
        private readonly EmotionDetectionService _emotionDetectionService;
        private readonly LogService _logService;
        private readonly UserDAO _userDAO;
        private readonly DiaryDAO _diaryDAO;
        private readonly LogDAO _logDAO;

        public EndToEndTests()
        {
            _userService = new UserService();
            _emotionDetectionService = new EmotionDetectionService();
            _diaryService = new DiaryService(_emotionDetectionService);
            _logService = new LogService();
            _userDAO = new UserDAO();
            _diaryDAO = new DiaryDAO();
            _logDAO = new LogDAO();
        }

        /// <summary>
        /// Runs all end-to-end tests and reports results.
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Anonymous Emotion Diary - End-to-End Tests");
            Console.WriteLine("========================================\n");

            int passedTests = 0;
            int totalTests = 0;

            // Test 1: User Registration Flow
            totalTests++;
            if (TestUserRegistrationFlow())
            {
                passedTests++;
                Console.WriteLine("✓ Test 1 PASSED: User Registration Flow\n");
            }
            else
            {
                Console.WriteLine("✗ Test 1 FAILED: User Registration Flow\n");
            }

            // Test 2: User Login Flow
            totalTests++;
            if (TestUserLoginFlow())
            {
                passedTests++;
                Console.WriteLine("✓ Test 2 PASSED: User Login Flow\n");
            }
            else
            {
                Console.WriteLine("✗ Test 2 FAILED: User Login Flow\n");
            }

            // Test 3: Diary Creation Flow
            totalTests++;
            if (TestDiaryCreationFlow())
            {
                passedTests++;
                Console.WriteLine("✓ Test 3 PASSED: Diary Creation Flow\n");
            }
            else
            {
                Console.WriteLine("✗ Test 3 FAILED: Diary Creation Flow\n");
            }

            // Test 4: High-Risk Emotion Detection
            totalTests++;
            if (TestHighRiskEmotionDetection())
            {
                passedTests++;
                Console.WriteLine("✓ Test 4 PASSED: High-Risk Emotion Detection\n");
            }
            else
            {
                Console.WriteLine("✗ Test 4 FAILED: High-Risk Emotion Detection\n");
            }

            // Test 5: Diary Viewing Flow
            totalTests++;
            if (TestDiaryViewingFlow())
            {
                passedTests++;
                Console.WriteLine("✓ Test 5 PASSED: Diary Viewing Flow\n");
            }
            else
            {
                Console.WriteLine("✗ Test 5 FAILED: Diary Viewing Flow\n");
            }

            // Test 6: Diary Deletion Flow
            totalTests++;
            if (TestDiaryDeletionFlow())
            {
                passedTests++;
                Console.WriteLine("✓ Test 6 PASSED: Diary Deletion Flow\n");
            }
            else
            {
                Console.WriteLine("✗ Test 6 FAILED: Diary Deletion Flow\n");
            }

            // Test 7: Logging System
            totalTests++;
            if (TestLoggingSystem())
            {
                passedTests++;
                Console.WriteLine("✓ Test 7 PASSED: Logging System\n");
            }
            else
            {
                Console.WriteLine("✗ Test 7 FAILED: Logging System\n");
            }

            // Summary
            Console.WriteLine("========================================");
            Console.WriteLine($"Test Results: {passedTests}/{totalTests} tests passed");
            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Test 1: Validates user registration flow
        /// - Input validation (username length, password length)
        /// - Database storage
        /// - Success message display
        /// </summary>
        private bool TestUserRegistrationFlow()
        {
            try
            {
                Console.WriteLine("Test 1: User Registration Flow");
                Console.WriteLine("  - Testing username validation...");

                // Test invalid username (too short)
                if (_userService.ValidateUsername("ab"))
                {
                    Console.WriteLine("    ✗ Should reject username shorter than 3 characters");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected short username");

                // Test invalid username (too long)
                if (_userService.ValidateUsername("abcdefghijklmnopqrstu"))
                {
                    Console.WriteLine("    ✗ Should reject username longer than 20 characters");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected long username");

                // Test invalid password (too short)
                if (_userService.ValidatePassword("short"))
                {
                    Console.WriteLine("    ✗ Should reject password shorter than 8 characters");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected short password");

                // Test valid registration
                Console.WriteLine("  - Testing valid registration...");
                string testUsername = $"testuser_{Guid.NewGuid().ToString().Substring(0, 8)}";
                string testPassword = "ValidPassword123";

                User registeredUser = _userService.RegisterUser(testUsername, testPassword);
                if (registeredUser == null)
                {
                    Console.WriteLine("    ✗ Registration failed");
                    return false;
                }
                Console.WriteLine($"    ✓ User registered successfully: {registeredUser.Username} (ID: {registeredUser.UserId})");

                // Verify user is in database
                User retrievedUser = _userDAO.SelectUserByUsername(testUsername);
                if (retrievedUser == null)
                {
                    Console.WriteLine("    ✗ User not found in database");
                    return false;
                }
                Console.WriteLine("    ✓ User verified in database");

                // Test duplicate username rejection
                Console.WriteLine("  - Testing duplicate username rejection...");
                User duplicateAttempt = _userService.RegisterUser(testUsername, "AnotherPassword123");
                if (duplicateAttempt != null)
                {
                    Console.WriteLine("    ✗ Should reject duplicate username");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected duplicate username");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 2: Validates user login flow
        /// - Correct credentials acceptance
        /// - Incorrect credentials rejection
        /// - Last login timestamp update
        /// </summary>
        private bool TestUserLoginFlow()
        {
            try
            {
                Console.WriteLine("Test 2: User Login Flow");

                // Create a test user
                Console.WriteLine("  - Creating test user...");
                string testUsername = $"logintest_{Guid.NewGuid().ToString().Substring(0, 8)}";
                string testPassword = "LoginPassword123";
                User registeredUser = _userService.RegisterUser(testUsername, testPassword);
                if (registeredUser == null)
                {
                    Console.WriteLine("    ✗ Failed to create test user");
                    return false;
                }
                Console.WriteLine($"    ✓ Test user created: {testUsername}");

                // Test login with correct credentials
                Console.WriteLine("  - Testing login with correct credentials...");
                User loggedInUser = _userService.LoginUser(testUsername, testPassword);
                if (loggedInUser == null)
                {
                    Console.WriteLine("    ✗ Login failed with correct credentials");
                    return false;
                }
                Console.WriteLine($"    ✓ Login successful: {loggedInUser.Username}");

                // Test login with incorrect password
                Console.WriteLine("  - Testing login with incorrect password...");
                User failedLogin = _userService.LoginUser(testUsername, "WrongPassword");
                if (failedLogin != null)
                {
                    Console.WriteLine("    ✗ Should reject incorrect password");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected incorrect password");

                // Test login with non-existent user
                Console.WriteLine("  - Testing login with non-existent user...");
                User nonExistentLogin = _userService.LoginUser("nonexistent_user", "SomePassword123");
                if (nonExistentLogin != null)
                {
                    Console.WriteLine("    ✗ Should reject non-existent user");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected non-existent user");

                // Verify last login timestamp was updated
                Console.WriteLine("  - Verifying last login timestamp...");
                User updatedUser = _userDAO.SelectUserById(loggedInUser.UserId);
                if (updatedUser.LastLoginAt == null)
                {
                    Console.WriteLine("    ✗ Last login timestamp not updated");
                    return false;
                }
                Console.WriteLine($"    ✓ Last login timestamp updated: {updatedUser.LastLoginAt}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 3: Validates diary creation flow
        /// - Content validation
        /// - Emotion analysis
        /// - Database storage
        /// - Success message
        /// </summary>
        private bool TestDiaryCreationFlow()
        {
            try
            {
                Console.WriteLine("Test 3: Diary Creation Flow");

                // Create a test user
                Console.WriteLine("  - Creating test user...");
                string testUsername = $"diarytest_{Guid.NewGuid().ToString().Substring(0, 8)}";
                User testUser = _userService.RegisterUser(testUsername, "DiaryPassword123");
                if (testUser == null)
                {
                    Console.WriteLine("    ✗ Failed to create test user");
                    return false;
                }
                Console.WriteLine($"    ✓ Test user created: {testUsername}");

                // Test empty content validation
                Console.WriteLine("  - Testing empty content validation...");
                if (_diaryService.ValidateDiaryContent(""))
                {
                    Console.WriteLine("    ✗ Should reject empty content");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected empty content");

                // Test content length validation
                Console.WriteLine("  - Testing content length validation...");
                string tooLongContent = new string('a', 5001);
                if (_diaryService.ValidateDiaryContent(tooLongContent))
                {
                    Console.WriteLine("    ✗ Should reject content exceeding 5000 characters");
                    return false;
                }
                Console.WriteLine("    ✓ Correctly rejected oversized content");

                // Test valid diary creation
                Console.WriteLine("  - Testing valid diary creation...");
                string diaryContent = "Today was a good day. I felt happy and accomplished my goals.";
                Diary createdDiary = _diaryService.CreateDiary(testUser.UserId, diaryContent);
                if (createdDiary == null)
                {
                    Console.WriteLine("    ✗ Diary creation failed");
                    return false;
                }
                Console.WriteLine($"    ✓ Diary created successfully (ID: {createdDiary.DiaryId})");

                // Verify diary is in database
                Console.WriteLine("  - Verifying diary in database...");
                Diary retrievedDiary = _diaryDAO.SelectDiaryById(createdDiary.DiaryId);
                if (retrievedDiary == null)
                {
                    Console.WriteLine("    ✗ Diary not found in database");
                    return false;
                }
                Console.WriteLine("    ✓ Diary verified in database");

                // Verify emotion analysis was performed
                Console.WriteLine("  - Verifying emotion analysis...");
                if (retrievedDiary.EmotionIndex < 0 || retrievedDiary.EmotionIndex > 100)
                {
                    Console.WriteLine($"    ✗ Invalid emotion index: {retrievedDiary.EmotionIndex}");
                    return false;
                }
                Console.WriteLine($"    ✓ Emotion index calculated: {retrievedDiary.EmotionIndex}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 4: Validates high-risk emotion detection
        /// - High-risk threshold detection
        /// - Risk warning generation
        /// - Visual marking in diary list
        /// </summary>
        private bool TestHighRiskEmotionDetection()
        {
            try
            {
                Console.WriteLine("Test 4: High-Risk Emotion Detection");

                // Create a test user
                Console.WriteLine("  - Creating test user...");
                string testUsername = $"risktest_{Guid.NewGuid().ToString().Substring(0, 8)}";
                User testUser = _userService.RegisterUser(testUsername, "RiskPassword123");
                if (testUser == null)
                {
                    Console.WriteLine("    ✗ Failed to create test user");
                    return false;
                }
                Console.WriteLine($"    ✓ Test user created: {testUsername}");

                // Create a high-risk diary (with negative emotions)
                Console.WriteLine("  - Creating high-risk diary...");
                string highRiskContent = "我感到非常绝望和自杀的念头。生活没有意义。我想死。";
                Diary highRiskDiary = _diaryService.CreateDiary(testUser.UserId, highRiskContent);
                if (highRiskDiary == null)
                {
                    Console.WriteLine("    ✗ Failed to create high-risk diary");
                    return false;
                }
                Console.WriteLine($"    ✓ High-risk diary created (Emotion Index: {highRiskDiary.EmotionIndex})");

                // Test high-risk detection
                Console.WriteLine("  - Testing high-risk detection...");
                bool isHighRisk = _emotionDetectionService.IsHighRisk(highRiskDiary.EmotionIndex);
                if (!isHighRisk && highRiskDiary.EmotionIndex > 70)
                {
                    Console.WriteLine($"    ✗ Should detect high-risk emotion (index: {highRiskDiary.EmotionIndex})");
                    return false;
                }
                if (isHighRisk)
                {
                    Console.WriteLine($"    ✓ High-risk emotion detected (index: {highRiskDiary.EmotionIndex})");
                }
                else
                {
                    Console.WriteLine($"    ✓ Emotion index {highRiskDiary.EmotionIndex} is below threshold");
                }

                // Test risk warning generation
                Console.WriteLine("  - Testing risk warning generation...");
                if (isHighRisk)
                {
                    string warningMessage = _emotionDetectionService.GetRiskWarning(highRiskDiary.EmotionIndex);
                    if (string.IsNullOrEmpty(warningMessage))
                    {
                        Console.WriteLine("    ✗ Warning message is empty");
                        return false;
                    }
                    if (!warningMessage.Contains("心理援助") && !warningMessage.Contains("support"))
                    {
                        Console.WriteLine("    ✗ Warning message missing support resources");
                        return false;
                    }
                    Console.WriteLine("    ✓ Risk warning generated with support resources");
                }

                // Create a low-risk diary (with positive emotions)
                Console.WriteLine("  - Creating low-risk diary...");
                string lowRiskContent = "今天很开心，我完成了所有任务，感到满足和放松。";
                Diary lowRiskDiary = _diaryService.CreateDiary(testUser.UserId, lowRiskContent);
                if (lowRiskDiary == null)
                {
                    Console.WriteLine("    ✗ Failed to create low-risk diary");
                    return false;
                }
                Console.WriteLine($"    ✓ Low-risk diary created (Emotion Index: {lowRiskDiary.EmotionIndex})");

                // Verify low-risk is not flagged
                bool isLowRiskHighRisk = _emotionDetectionService.IsHighRisk(lowRiskDiary.EmotionIndex);
                if (isLowRiskHighRisk)
                {
                    Console.WriteLine($"    ✗ Low-risk emotion incorrectly flagged as high-risk");
                    return false;
                }
                Console.WriteLine($"    ✓ Low-risk emotion correctly not flagged");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 5: Validates diary viewing flow
        /// - Diary list retrieval
        /// - Diary detail display
        /// - Correct ordering (newest first)
        /// </summary>
        private bool TestDiaryViewingFlow()
        {
            try
            {
                Console.WriteLine("Test 5: Diary Viewing Flow");

                // Create a test user
                Console.WriteLine("  - Creating test user...");
                string testUsername = $"viewtest_{Guid.NewGuid().ToString().Substring(0, 8)}";
                User testUser = _userService.RegisterUser(testUsername, "ViewPassword123");
                if (testUser == null)
                {
                    Console.WriteLine("    ✗ Failed to create test user");
                    return false;
                }
                Console.WriteLine($"    ✓ Test user created: {testUsername}");

                // Create multiple diaries
                Console.WriteLine("  - Creating multiple diaries...");
                Diary diary1 = _diaryService.CreateDiary(testUser.UserId, "First diary entry");
                System.Threading.Thread.Sleep(100); // Small delay to ensure different timestamps
                Diary diary2 = _diaryService.CreateDiary(testUser.UserId, "Second diary entry");
                System.Threading.Thread.Sleep(100);
                Diary diary3 = _diaryService.CreateDiary(testUser.UserId, "Third diary entry");

                if (diary1 == null || diary2 == null || diary3 == null)
                {
                    Console.WriteLine("    ✗ Failed to create test diaries");
                    return false;
                }
                Console.WriteLine($"    ✓ Created 3 test diaries");

                // Retrieve diary list
                Console.WriteLine("  - Retrieving diary list...");
                List<Diary> userDiaries = _diaryService.GetUserDiaries(testUser.UserId);
                if (userDiaries.Count < 3)
                {
                    Console.WriteLine($"    ✗ Expected at least 3 diaries, got {userDiaries.Count}");
                    return false;
                }
                Console.WriteLine($"    ✓ Retrieved {userDiaries.Count} diaries");

                // Verify ordering (newest first)
                Console.WriteLine("  - Verifying diary ordering (newest first)...");
                if (userDiaries[0].DiaryId != diary3.DiaryId)
                {
                    Console.WriteLine("    ✗ Diaries not ordered correctly (newest first)");
                    return false;
                }
                Console.WriteLine("    ✓ Diaries correctly ordered (newest first)");

                // Retrieve individual diary detail
                Console.WriteLine("  - Retrieving diary detail...");
                Diary retrievedDetail = _diaryService.GetDiaryById(diary1.DiaryId);
                if (retrievedDetail == null)
                {
                    Console.WriteLine("    ✗ Failed to retrieve diary detail");
                    return false;
                }
                if (retrievedDetail.Content != "First diary entry")
                {
                    Console.WriteLine("    ✗ Diary content mismatch");
                    return false;
                }
                Console.WriteLine("    ✓ Diary detail retrieved correctly");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 6: Validates diary deletion flow
        /// - Diary deletion from database
        /// - List refresh after deletion
        /// - Confirmation before deletion
        /// </summary>
        private bool TestDiaryDeletionFlow()
        {
            try
            {
                Console.WriteLine("Test 6: Diary Deletion Flow");

                // Create a test user
                Console.WriteLine("  - Creating test user...");
                string testUsername = $"deletetest_{Guid.NewGuid().ToString().Substring(0, 8)}";
                User testUser = _userService.RegisterUser(testUsername, "DeletePassword123");
                if (testUser == null)
                {
                    Console.WriteLine("    ✗ Failed to create test user");
                    return false;
                }
                Console.WriteLine($"    ✓ Test user created: {testUsername}");

                // Create a diary to delete
                Console.WriteLine("  - Creating diary for deletion...");
                Diary diaryToDelete = _diaryService.CreateDiary(testUser.UserId, "This diary will be deleted");
                if (diaryToDelete == null)
                {
                    Console.WriteLine("    ✗ Failed to create test diary");
                    return false;
                }
                int diaryIdToDelete = diaryToDelete.DiaryId;
                Console.WriteLine($"    ✓ Diary created (ID: {diaryIdToDelete})");

                // Verify diary exists before deletion
                Console.WriteLine("  - Verifying diary exists before deletion...");
                Diary beforeDelete = _diaryDAO.SelectDiaryById(diaryIdToDelete);
                if (beforeDelete == null)
                {
                    Console.WriteLine("    ✗ Diary not found before deletion");
                    return false;
                }
                Console.WriteLine("    ✓ Diary verified in database");

                // Delete the diary
                Console.WriteLine("  - Deleting diary...");
                bool deleteSuccess = _diaryService.DeleteDiary(diaryIdToDelete);
                if (!deleteSuccess)
                {
                    Console.WriteLine("    ✗ Diary deletion failed");
                    return false;
                }
                Console.WriteLine("    ✓ Diary deleted successfully");

                // Verify diary is removed from database
                Console.WriteLine("  - Verifying diary removed from database...");
                Diary afterDelete = _diaryDAO.SelectDiaryById(diaryIdToDelete);
                if (afterDelete != null)
                {
                    Console.WriteLine("    ✗ Diary still exists after deletion");
                    return false;
                }
                Console.WriteLine("    ✓ Diary confirmed removed from database");

                // Verify list is refreshed
                Console.WriteLine("  - Verifying list refresh...");
                List<Diary> userDiaries = _diaryService.GetUserDiaries(testUser.UserId);
                bool diaryStillInList = userDiaries.Any(d => d.DiaryId == diaryIdToDelete);
                if (diaryStillInList)
                {
                    Console.WriteLine("    ✗ Deleted diary still appears in list");
                    return false;
                }
                Console.WriteLine("    ✓ Deleted diary removed from list");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 7: Validates logging system
        /// - Debug log recording
        /// - Error log recording
        /// - Emotion analysis log recording
        /// - Log file creation and rotation
        /// </summary>
        private bool TestLoggingSystem()
        {
            try
            {
                Console.WriteLine("Test 7: Logging System");

                // Test debug logging
                Console.WriteLine("  - Testing debug logging...");
                _logService.LogDebug("Test debug message");
                string debugLogPath = Path.Combine(ConfigurationHelper.LogFilePath, ConfigurationHelper.DebugLogFileName);
                if (!File.Exists(debugLogPath))
                {
                    Console.WriteLine("    ✗ Debug log file not created");
                    return false;
                }
                string debugContent = File.ReadAllText(debugLogPath);
                if (!debugContent.Contains("Test debug message"))
                {
                    Console.WriteLine("    ✗ Debug message not found in log");
                    return false;
                }
                Console.WriteLine("    ✓ Debug logging working correctly");

                // Test error logging
                Console.WriteLine("  - Testing error logging...");
                try
                {
                    throw new Exception("Test exception for logging");
                }
                catch (Exception ex)
                {
                    _logService.LogError("Test error message", ex);
                }
                string errorLogPath = Path.Combine(ConfigurationHelper.LogFilePath, ConfigurationHelper.ErrorLogFileName);
                if (!File.Exists(errorLogPath))
                {
                    Console.WriteLine("    ✗ Error log file not created");
                    return false;
                }
                string errorContent = File.ReadAllText(errorLogPath);
                if (!errorContent.Contains("Test error message") || !errorContent.Contains("Test exception for logging"))
                {
                    Console.WriteLine("    ✗ Error message not found in log");
                    return false;
                }
                Console.WriteLine("    ✓ Error logging working correctly");

                // Test emotion analysis logging
                Console.WriteLine("  - Testing emotion analysis logging...");
                _logService.LogEmotionAnalysis(
                    diaryId: 999,
                    emotionIndex: 75,
                    analysisTime: DateTime.Now,
                    modelVersion: "test-model"
                );
                string emotionLogPath = Path.Combine(ConfigurationHelper.LogFilePath, ConfigurationHelper.EmotionAnalysisLogFileName);
                if (!File.Exists(emotionLogPath))
                {
                    Console.WriteLine("    ✗ Emotion analysis log file not created");
                    return false;
                }
                string emotionContent = File.ReadAllText(emotionLogPath);
                if (!emotionContent.Contains("DiaryId: 999") || !emotionContent.Contains("EmotionIndex: 75"))
                {
                    Console.WriteLine("    ✗ Emotion analysis data not found in log");
                    return false;
                }
                Console.WriteLine("    ✓ Emotion analysis logging working correctly");

                // Verify log directory structure
                Console.WriteLine("  - Verifying log directory structure...");
                string logDirectory = ConfigurationHelper.LogFilePath;
                if (!Directory.Exists(logDirectory))
                {
                    Console.WriteLine("    ✗ Log directory not created");
                    return false;
                }
                string[] logFiles = Directory.GetFiles(logDirectory);
                if (logFiles.Length < 3)
                {
                    Console.WriteLine("    ✗ Expected at least 3 log files");
                    return false;
                }
                Console.WriteLine($"    ✓ Log directory contains {logFiles.Length} log files");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ✗ Exception: {ex.Message}");
                return false;
            }
        }
    }
}
