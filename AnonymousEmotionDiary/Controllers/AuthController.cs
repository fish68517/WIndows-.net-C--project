using System;
using System.Windows.Forms;
using AnonymousEmotionDiary.Models;
using AnonymousEmotionDiary.Services;
using AnonymousEmotionDiary.Views;

namespace AnonymousEmotionDiary.Controllers
{
    /// <summary>
    /// 用于处理用户认证相关操作的控制器。
    /// 管理用户注册、登录与退出登录流程。
    /// </summary>
    public class AuthController
    {
        private readonly UserService _userService;
        private readonly LogService _logService;

        /// <summary>
        /// 初始化 AuthController 的新实例。
        /// </summary>
        public AuthController()
        {
            _userService = new UserService();
            _logService = new LogService();
        }

        /// <summary>
        /// 处理用户注册：校验输入，调用 UserService 完成注册，并提示相应结果。
        /// </summary>
        /// <param name="username">新账号的用户名。</param>
        /// <param name="password">新账号的密码。</param>
        /// <param name="confirmPassword">确认密码。</param>
        /// <returns>注册成功返回 true，否则返回 false。</returns>
        public bool HandleRegister(string username, string password, string confirmPassword)
        {
            try
            {
                // 校验输入不能为空
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    _logService.LogDebug("注册失败：输入字段为空");
                    MessageBox.Show("请填写所有字段。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 校验用户名格式
                if (!_userService.ValidateUsername(username))
                {
                    _logService.LogDebug($"注册失败：用户名格式不合法：'{username}'");
                    MessageBox.Show("用户名长度需为 3-20 位，只能包含字母、数字和下划线。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 校验密码格式
                if (!_userService.ValidatePassword(password))
                {
                    _logService.LogDebug("注册失败：密码格式不合法");
                    MessageBox.Show("密码长度至少 8 位。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 校验两次密码一致
                if (password != confirmPassword)
                {
                    _logService.LogDebug("注册失败：两次输入的密码不一致");
                    MessageBox.Show("两次输入的密码不一致。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 尝试注册
                User newUser = _userService.RegisterUser(username, password);

                if (newUser != null)
                {
                    _logService.LogDebug($"注册成功：用户 {username}");
                    MessageBox.Show($"注册成功！欢迎你，{newUser.Username}。现在可以登录了。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    _logService.LogDebug($"注册失败：UserService 返回 null，用户名：'{username}'");
                    MessageBox.Show("注册失败：用户名可能已存在，或发生了其他错误。", "注册失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"注册过程中发生异常，用户名：'{username}'", ex);
                MessageBox.Show("注册过程中发生未知错误，请稍后重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 处理用户登录：校验输入，调用 UserService 进行认证，成功后保存会话。
        /// </summary>
        /// <param name="username">要认证的用户名。</param>
        /// <param name="password">要校验的密码。</param>
        /// <returns>认证成功返回 User 对象，否则返回 null。</returns>
        public User HandleLogin(string username, string password)
        {
            try
            {
                // 校验输入不能为空
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logService.LogDebug("登录失败：用户名或密码为空");
                    MessageBox.Show("请输入用户名和密码。", "校验错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // 尝试登录
                User authenticatedUser = _userService.LoginUser(username, password);

                if (authenticatedUser != null)
                {
                    // 保存用户会话
                    SessionManager.SetCurrentUser(authenticatedUser);
                    _logService.LogDebug($"登录成功：用户 {username}，已保存会话");
                    return authenticatedUser;
                }
                else
                {
                    _logService.LogDebug($"登录失败：用户名 '{username}' 凭证无效");
                    MessageBox.Show("用户名或密码错误。", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"登录过程中发生异常，用户名：'{username}'", ex);
                MessageBox.Show("登录过程中发生未知错误，请稍后重试。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 处理用户退出登录：清理会话并返回登录界面。
        /// </summary>
        /// <returns>退出登录成功返回 true，否则返回 false。</returns>
        public bool HandleLogout()
        {
            try
            {
                string currentUsername = SessionManager.GetCurrentUsername();

                // 清理会话
                SessionManager.ClearSession();
                _logService.LogDebug($"用户退出登录成功：{currentUsername}");

                // 显示退出提示
                MessageBox.Show("已成功退出登录。", "退出登录", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("退出登录过程中发生异常", ex);
                MessageBox.Show("退出登录时发生错误。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
