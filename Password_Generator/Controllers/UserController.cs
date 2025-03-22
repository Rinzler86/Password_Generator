using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Password_Generator.Data;
using Password_Generator.Helpers;
using Password_Generator.Models;
using System.Security.Claims;

namespace Password_Generator.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Index action called. User email: {UserEmail}", userEmail);

            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {UserEmail}", userEmail);
                return NotFound();
            }

            // Query all vendor password records for this user.
            // For each vendor record, if there is at least one history record,
            // use the latest history's CreatedAt; otherwise, use the vendor's CreatedAt.
            var vendorDTOs = _context.VendorPasswords
                .Where(vp => vp.UserId == user.Id)
                .Select(vp => new VendorPasswordDTO
                {
                    Id = vp.Id,
                    VendorName = vp.VendorName,
                    CurrentPassword = vp.CurrentPassword,
                    DateCreated = vp.PasswordHistories.Any()
                        ? vp.PasswordHistories.OrderByDescending(ph => ph.CreatedAt).FirstOrDefault().CreatedAt
                        : vp.CreatedAt
                })
                .ToList();


            var viewModel = new UserDashboardViewModel
            {
                User = user,
                VendorPasswords = vendorDTOs
            };

            return View(viewModel);
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            _logger.LogInformation("Register GET action called.");
            return View();
        }

        [HttpPost("Register")]
        public IActionResult Register(User_Password_Generator user)
        {
            _logger.LogInformation("Register POST action called.");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid. Registering user: {UserEmail}", user.Email);

                // Hash the password before saving the user
                user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.HashedPassword);

                // Save the user to the database
                _context.Password_Generator_Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("ShowSuccessAndRedirect");
            }

            _logger.LogWarning("Model state is invalid. Registration failed for user: {UserEmail}", user.Email);
            TempData["ErrorMessage"] = "Registration failed. Please check the form for errors.";
            return View(user);
        }

        [HttpGet("ShowSuccessAndRedirect")]
        public IActionResult ShowSuccessAndRedirect()
        {
            return View();
        }


        [HttpGet("Login")]
        public IActionResult Login()
        {
            _logger.LogInformation("Login GET action called.");
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login POST action called.");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid. Attempting to log in user: {UserEmail}", model.Email);

                // Retrieve the user from the database
                var dbUser = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == model.Email);

                if (dbUser != null)
                {
                    _logger.LogInformation("User found in database: {UserEmail}", model.Email);

                    // Verify the password
                    if (BCrypt.Net.BCrypt.Verify(model.Password, dbUser.HashedPassword))
                    {
                        _logger.LogInformation("Password verification successful for user: {UserEmail}", model.Email);

                        // Authentication successful
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dbUser.FirstName + " " + dbUser.LastName),
                    new Claim(ClaimTypes.Email, dbUser.Email)
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        _logger.LogInformation("User logged in successfully: {UserEmail}", model.Email);
                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        _logger.LogWarning("Password verification failed for user: {UserEmail}", model.Email);
                        ModelState.AddModelError(string.Empty, "Invalid password.");
                    }
                }
                else
                {
                    _logger.LogWarning("User not found in database: {UserEmail}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid for user: {UserEmail}", model.Email);
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogWarning("ModelState error in {Key}: {ErrorMessage}", modelStateKey, error.ErrorMessage);
                    }
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string email, string answer1, string answer2, string newPassword, string confirmPassword)
        {
            _logger.LogInformation("ResetPassword POST action called for email: {Email}", email);

            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return Json(new { success = false, message = "User not found." });
            }

            if (user.ResetAnswer1.ToLower() == answer1.ToLower() && user.ResetAnswer2.ToLower() == answer2.ToLower())
            {
                if (newPassword == confirmPassword)
                {
                    user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Password reset successful!" });
                }
                else
                {
                    return Json(new { success = false, message = "Passwords do not match." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Security answers do not match." });
            }
        }

        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            _logger.LogInformation("ForgotPassword POST action called for email: {Email}", email);

            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return Json(new { success = false, message = "User not found." });
            }

            return Json(new
            {
                success = true,
                question1 = user.ResetQuestion1.GetDisplayName(),
                question2 = user.ResetQuestion2.GetDisplayName()
            });
        }

    }
}

