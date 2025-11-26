using HelpDeskKyotera.Models;
using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace HelpDeskKyotera.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountService(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IEmailService emailService,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true,
                PhoneNumber = model.PhoneNumber,
                CreatedOn = DateTime.UtcNow
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return result;

            // Assign "User" role by default
            IdentityResult roleAssignResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleAssignResult.Succeeded)
            {
                // Handle error - optionally return this failure instead
                // or log the issue and continue
                return roleAssignResult;
            }

            var token = await GenerateEmailConfirmationTokenAsync(user);

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={token}";

            await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.FirstName, confirmationLink);

            return result;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty || string.IsNullOrEmpty(token))
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token or user ID." });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var decodedBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
                var loginLink = $"{baseUrl}/Account/Login";

                await _emailService.SendAccountCreatedEmailAsync(user.Email!, user.FirstName!, loginLink);
            }

            return result;
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return SignInResult.Failed;

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return SignInResult.NotAllowed;

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Update LastLogin
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task SendEmailConfirmationAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Prevent user enumeration by not disclosing existence
                return;
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                // Email already confirmed; no action needed
                return;
            }

            var token = await GenerateEmailConfirmationTokenAsync(user);

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={token}";

            await _emailService.SendResendConfirmationEmailAsync(user.Email!, user.FirstName!, confirmationLink);
        }

        public async Task<ProfileViewModel> GetUserProfileByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new ArgumentException("User not found.", nameof(email));

            return new ProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LastLoggedIn = user.LastLogin,
                CreatedOn = user.CreatedOn
            };
        }

        //Helper Method
        private async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            return encodedToken;
        }
    }
}