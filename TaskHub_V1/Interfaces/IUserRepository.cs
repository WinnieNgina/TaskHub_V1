using TaskHub_V1.Models;
using Microsoft.AspNetCore.Identity;

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
