using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Users;
using HelpDeskKyotera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskKyotera.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        // GET: /Account/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel();
            
            // Get all available roles except Admin
            using var scope = HttpContext.RequestServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            
            var allRoles = await roleManager.Roles
                .Where(r => r.Name != "Admin")
                .ToListAsync();
            
            var allDepartments = await departmentService.GetAllDepartmentsAsync();
            
            model.AvailableRoles = allRoles.Select(r => new RoleCheckboxItem 
            { 
                RoleId = r.Id, 
                RoleName = r.Name ?? "", 
                IsSelected = false 
            }).ToList();
            
            model.AvailableDepartments = allDepartments;
            
            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload available roles and departments on validation error
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                    var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
                    
                    var allRoles = await roleManager.Roles
                        .Where(r => r.Name != "Admin")
                        .ToListAsync();
                    
                    var allDepartments = await departmentService.GetAllDepartmentsAsync();
                    
                    model.AvailableRoles = allRoles.Select(r => new RoleCheckboxItem 
                    { 
                        RoleId = r.Id, 
                        RoleName = r.Name ?? "", 
                        IsSelected = model.SelectedRoleIds?.Contains(r.Id) ?? false 
                    }).ToList();
                    
                    model.AvailableDepartments = allDepartments;
                    
                    return View(model);
                }

                var result = await _accountService.RegisterUserAsync(model);

                if (result.Succeeded)
                    return RedirectToAction("RegistrationConfirmation");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                // Reload available roles and departments on registration error
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                    var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
                    
                    var allRoles = await roleManager.Roles
                        .Where(r => r.Name != "Admin")
                        .ToListAsync();
                    
                    var allDepartments = await departmentService.GetAllDepartmentsAsync();
                    
                    model.AvailableRoles = allRoles.Select(r => new RoleCheckboxItem 
                    { 
                        RoleId = r.Id, 
                        RoleName = r.Name ?? "", 
                        IsSelected = model.SelectedRoleIds?.Contains(r.Id) ?? false 
                    }).ToList();
                    
                    model.AvailableDepartments = allDepartments;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        // GET: /Account/RegistrationConfirmation
        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {
            return View();
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            try
            {
                if (userId == Guid.Empty || string.IsNullOrEmpty(token))
                    return BadRequest("Invalid email confirmation request.");

                var result = await _accountService.ConfirmEmailAsync(userId, token);

                if (result.Succeeded)
                    return View("EmailConfirmed");

                // Combine errors into one message or pass errors to the view
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for UserId: {UserId}", userId);
                ModelState.AddModelError("", "An unexpected error occurred during email confirmation.");
                return View("Error");
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var result = await _accountService.LoginUserAsync(model);

                if (result.Succeeded)
                {
                    // After successful sign-in, check the user's roles and redirect Admins to the Admin Dashboard.
                    // Add diagnostic logging to help figure out why redirect may not occur.
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var signedInUser = await userManager.FindByEmailAsync(model.Email);

                    if (signedInUser == null)
                    {
                        _logger.LogWarning("Login succeeded but user lookup by email failed for '{Email}'", model.Email);
                    }
                    else
                    {
                        var roles = await userManager.GetRolesAsync(signedInUser);
                        _logger.LogInformation("User '{Email}' roles after sign-in: {Roles}", model.Email, string.Join(',', roles));

                        var isAdmin = roles != null && roles.Contains("Admin");
                        _logger.LogInformation("IsAdmin check for '{Email}': {IsAdmin}", model.Email, isAdmin);

                        if (isAdmin || await userManager.IsInRoleAsync(signedInUser, "Admin"))
                        {
                            _logger.LogInformation("Redirecting admin user '{Email}' to AdminDashboard", model.Email);
                            return RedirectToAction("Index", "AdminDashboard");
                        }
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "UserDashboard");
                }

                if (result.IsNotAllowed)
                    ModelState.AddModelError("", "Email is not confirmed yet.");
                else
                    ModelState.AddModelError("", "Invalid login attempt.");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            try
            {
                var model = await _accountService.GetUserProfileByEmailAsync(email);
                return View(model);
            }
            catch (ArgumentException)
            {
                return View("Error");
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _accountService.LogoutUserAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Optionally redirect to error page or home with message
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/ResendEmailConfirmation
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        // POST: /Account/ResendEmailConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendConfirmationEmailViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _accountService.SendEmailConfirmationAsync(model.Email);

                ViewBag.Message = "If the email is registered, a confirmation link has been sent.";
                return View("ResendEmailConfirmationSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email confirmation to: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }
    }
}