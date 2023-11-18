
using TaskHub_V1.Data;
using TaskHub_V1.Interfaces;
using TaskHub_V1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskHub_V1.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public UserRepository(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public User GetUserbyId(string userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }

        public User GetUserbyEmail(string email)
        {
            return _context.Users.Where(u => u.Email == email).FirstOrDefault();
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.Where(u => u.UserName == username).FirstOrDefault();
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.OrderBy(u => u.Id).ToList();
        }

        public bool UserExists(string userId)
        {
            return _context.Users.Any(u => u.Id == userId);
        }

        public ICollection<ProjectTasks> GetUserTaskList(string userId)
        {
            return _context.ProjectTasks.Where(p => p.User.Id == userId).OrderByDescending(pt => pt.CreatedAt).ToList();
        }

        public ICollection<Project> GetProjectsManagedbyUser(string userId)
        {
            return _context.Projects.Where(p => p.ProjectManager.Id == userId).OrderByDescending(p => p.CreatedAt).ToList();
        }
        public ICollection<Comment> GetCommentsByUser(string userId)
        {
            return _context.Comments.Where(c => c.User.Id == userId).OrderBy(c => c.CreatedAt).ToList();
        }

        public bool AddUserToProject(string userId, string projectId)
        {
            if (!_context.Users.Any(u => u.Id == userId) || !_context.Projects.Any(p => p.Id == projectId))
                return false;

            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId.ToString()
            };
            _context.UserProjects.Add(userProject);
            return Save();
        }

        public bool RemoveUserFromProject(string userId, string projectId)
        {
            var userProject = _context.UserProjects.FirstOrDefault(up => up.UserId == userId && up.ProjectId == projectId.ToString());
            if (userProject == null)
                return false;

            _context.UserProjects.Remove(userProject);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            // Implement logic to retrieve a user by their ID from the database
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            // Implement logic to update a user in the database
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(User user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRoles(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            // Generate the email confirmation token using the user management system or identity framework
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Optionally, you can modify or customize the token before returning it
            return token;
        }
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            // Use the user management system or identity framework to confirm the user's email
            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result;
        }
        // FindUserByEmailAsync: Find a user by email
        public async Task<User> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }

        // CheckPasswordAsync: Check if the provided password is correct for the user
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

        // GenerateAuthToken: Generate an authentication token for the user
        public string GenerateAuthToken(User user)
        {
            // Generate the authentication token using your preferred authentication mechanism
            // For example, you can use JWT (JSON Web Tokens) to generate the token

            // Here's an example using JWT:
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"]),
                    new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"])
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

    }
}
