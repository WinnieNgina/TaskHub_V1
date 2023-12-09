using Microsoft.AspNetCore.Identity;
using TaskHub_V1.Models;

namespace TaskHub_V1.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<User> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(User user);
        Task<IList<string>> GetUserRoles(User user);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<User> FindUserByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> CheckCurrentPasswordAsync(User user, string currentPassword);
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<bool> LockUserAccountAsync(User user);
        Task<bool> UnlockUserAccountAsync(User user);
        Task LogoutAsync();
        Task EnableTwoFactorAuthenticationAsync(User user);
        Task DisableTwoFactorAuthenticationAsync(User user);
        Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail);
        Task<IdentityResult> ChangeEmailAsync(User user, string newEmail, string emailChangeToken);
        Task SignInAsync(User user, bool isPersistent);
        string GenerateAuthToken(User user);

        ICollection<User> GetUsers();
        User GetUserbyId(string userId);
        User GetUserbyEmail(string email);
        User GetUserByUsername(string username);
        bool UserExists(string userId);
        ICollection<Comment> GetCommentsByUser(string userId);

        ICollection<ProjectTasks> GetUserTaskList(string userId);
        ICollection<Project> GetProjectsManagedbyUser(string userId);
        bool AddUserToProject(string userId, string projectId);
        bool RemoveUserFromProject(string userId, string projectId);
        bool Save();
    }
}
