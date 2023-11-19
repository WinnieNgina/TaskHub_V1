using TaskHub_V1.Interfaces;
using TaskHub_V1.Models;
using TaskHub_V1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace TaskHub_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;

        public UserController(IUserRepository userRepository, EmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ICollection<User>))]
        [Authorize]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            return Ok(users.Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.FirstName,
                x.LastName,
                x.PhoneNumber
            }));
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateModel model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicturePath = model.ProfilePicturePath
            };
            string password = model.Password;
            var result = await _userRepository.CreateUserAsync(user, password);

            if (result.Succeeded)
            {
                // Generate email confirmation token
                var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
                //Console.WriteLine($"User ID: {user.Id}");
                //Console.WriteLine($"Token: {token}");
                var callbackUrl = Url.Action("ConfirmEmail", "User", new
                {
                    userId = Uri.EscapeDataString(user.Id),
                    token = Uri.EscapeDataString(token)
                }, "https", Request.Host.Value);
                //Console.WriteLine($"Callback URL: {callbackUrl}");

                // Send confirmation email
                var subject = "Confirm your email";

                var message = $@"Please confirm your email by clicking the following link:
                {callbackUrl}

                If you're unable to click the link, please copy and paste it into your web browser.";


                await _emailService.SendEmailAsync(user.Email, subject, message);

                return Ok("User created successfully");
            }

            // Include error details in the response
            return BadRequest(new { Message = "Failed to create user", Errors = result.Errors });
        }
        
        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("User ID and confirmation code are required.");
            }
            var decodedUserId = Uri.UnescapeDataString(userId);
            var decodedToken = Uri.UnescapeDataString(token);
            // Find the user by their ID
            var user = await _userRepository.GetUserByIdAsync(decodedUserId);

            if (user == null)
            {
                // User not found
                return BadRequest("Invalid user ID");
            }

            // Verify the email confirmation token
            var result = await _userRepository.ConfirmEmailAsync(user, decodedToken);

            // Log the result of the email confirmation
            Console.WriteLine($"Email confirmation result: {result.Succeeded}");
            if (!result.Succeeded)
            {
                Console.WriteLine($"Email confirmation errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            if (result.Succeeded)
            {
                // Email confirmed successfully
                return Ok("Email confirmed successfully");
            }
            else
            {
                // Email confirmation failed
                return BadRequest("Failed to confirm email");
            }
        }
        
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userRepository.FindUserByEmailAsync(model.Email);

            if (user == null)
            {
                // User not found
                return BadRequest("Invalid email or password");
            }

            if (!user.EmailConfirmed)
            {
                // User's email is not confirmed
                return BadRequest("Please confirm your email before logging in");
            }

            var result = await _userRepository.CheckPasswordAsync(user, model.Password);

            if (result == true)
            {
                // Password is correct, generate and return the authentication token
                var token = _userRepository.GenerateAuthToken(user);

                return Ok(new { Token = token });
            }

            // Invalid password
            return BadRequest("Invalid email or password");
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // User not found
            }
            // Return a DTO or a model with the user's information
            return Ok(new { user.Id, user.UserName, user.Email });

        }
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRoles(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User Roles not found");
            }
            var roles = await _userRepository.GetUserRoles(user);
            return Ok(roles);
        }
        [HttpPut("{userId}")]

        public async Task<IActionResult> UpdateUser(string userId, UserUpdateModel model)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            var success = await _userRepository.UpdateUserAsync(user);
            if (success)
            {
                return Ok("User updated successfully");
            }
            return BadRequest("Failed to update user");
        }
        [HttpPost("{userId}/ChangePassWord")]
        public async Task<IActionResult> ChangePassword(string userId, ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            var isCurrentPasswordValid = await _userRepository.CheckCurrentPasswordAsync(user, model.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return BadRequest("The current password is incorrect.");
            }
            var result = await _userRepository.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _userRepository.SignInAsync(user, isPersistent: false);
                return Ok("Password changed successfully.");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{userId}")]

        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // User not found
            }
            var success = await _userRepository.DeleteUserAsync(user);
            if (success)
            {
                return Ok("User deleted successfully");
            }
            return NotFound("User not found or failed to delete user");
        }

    }
}
