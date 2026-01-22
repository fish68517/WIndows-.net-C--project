using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public UserService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<User> RegisterAsync(string email, string password, string nickname)
        {
            // Validate email uniqueness
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("该邮箱已被注册");
            }

            // Validate password strength
            if (!IsPasswordStrong(password))
            {
                throw new InvalidOperationException("密码必须至少8个字符，包含大小写字母和数字");
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Nickname = nickname,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpperCase && hasLowerCase && hasDigit;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string nickname, string bio, IFormFile avatar)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Nickname = nickname;
            user.Bio = bio;

            if (avatar != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> LogoutAsync()
        {
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string nickname = null, DateTime? registeredAfter = null, DateTime? registeredBefore = null)
        {
            var users = await _unitOfWork.Users.GetAllAsync();

            if (!string.IsNullOrEmpty(nickname))
            {
                users = users.Where(u => u.Nickname.Contains(nickname, StringComparison.OrdinalIgnoreCase));
            }

            if (registeredAfter.HasValue)
            {
                users = users.Where(u => u.CreatedAt >= registeredAfter.Value);
            }

            if (registeredBefore.HasValue)
            {
                users = users.Where(u => u.CreatedAt <= registeredBefore.Value);
            }

            return users.OrderByDescending(u => u.CreatedAt);
        }

        public async Task<bool> DisableUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EnableUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
