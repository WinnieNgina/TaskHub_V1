using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub_V1.Interfaces;
using TaskHub_V1.Models;
using TaskHub_V1.Services;

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
                    token = token // Do not use Uri.EscapeDataString for token here
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
            //Console.WriteLine($"Email confirmation result: {result.Succeeded}");
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
        [HttpPost("{userId}/Lock")]
        [Authorize]
        public async Task<IActionResult> LockUserAccount(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Lock the user account
            var lockResult = await _userRepository.LockUserAccountAsync(user);

            if (lockResult)
            {
                // Optionally, you may return a success message or user details.
                return Ok("User account locked successfully.");
            }
            else
            {
                return BadRequest("Failed to lock user account."); // You can customize the response based on your needs.
            }
        }
        [HttpPost("UnlockAccount")]
        [Authorize]
        public async Task<IActionResult> UnlockAccount(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var success = await _userRepository.UnlockUserAccountAsync(user);

            if (success)
            {
                return Ok("User account unlocked successfully.");
            }

            return BadRequest("Failed to unlock user account.");
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userRepository.LogoutAsync();
            return Ok("Logout successful.");
        }
        [HttpPost("{userId}/Enable2FA")]
        public async Task<IActionResult> EnableTwoFactorAuthentication(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            await _userRepository.EnableTwoFactorAuthenticationAsync(user);

            return Ok("Two-factor authentication enabled successfully.");
        }

        [HttpPost("{userId}/Disable2FA")]
        public async Task<IActionResult> DisableTwoFactorAuthentication(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            await _userRepository.DisableTwoFactorAuthenticationAsync(user);

            return Ok("Two-factor authentication disabled successfully.");
        }

        [HttpPost("{userId}/ChangeEmail")]
        public async Task<IActionResult> ChangeUserEmail(string userId, [FromBody] ChangeEmailModel model)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Verify the user's password before proceeding with the email change
            var passwordValid = await _userRepository.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                return BadRequest("Incorrect password. Email change request aborted.");
            }

            // Generate a unique email change token
            var emailChangeToken = await _userRepository.GenerateChangeEmailTokenAsync(user, model.NewEmail);

            if (string.IsNullOrEmpty(emailChangeToken))
            {
                return BadRequest("Failed to generate email change token.");
            }

            // Change the user's email address
            //var result = await _userRepository.ChangeEmailAsync(user, model.NewEmail, emailChangeToken);
            // Send a confirmation email to the new email address
            var newEmailConfirmationLink = Url.Action("ConfirmNewEmail", "User", new
            {
                userId = Uri.EscapeDataString(user.Id),
                token = emailChangeToken
            }, "https", Request.Host.Value);

            var newEmailSubject = "Email Change Confirmation";
            var newEmailMessage = $"We received a request to change the email address associated with your account. If you didn't make this request, please ignore this email.\n\nTo confirm the change, click the following link:\n{newEmailConfirmationLink}";

            await _emailService.SendEmailAsync(model.NewEmail, newEmailSubject, newEmailMessage);

            // Optionally, you may return a success message or additional steps required.
            return Ok("Email change request processed successfully. Confirmation email sent to the new email address.");
        }
        // Add this method to your UserController
        [HttpPost("ConfirmNewEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmNewEmail(string userId, string token, string NewEmail)
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

            // Verify the email change token
            var result = await _userRepository.ChangeEmailAsync(user, NewEmail, decodedToken);

            if (result.Succeeded)
            {
                // Email change confirmed successfully
                return Ok("Email change confirmed successfully");
            }
            else
            {
                // Email change confirmation failed
                return BadRequest("Failed to confirm email change");
            }
        }
    }
}
