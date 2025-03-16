using Microsoft.AspNetCore.Mvc;
using Password_Generator.Data;
using Password_Generator.Models;
using System.Security.Claims;

namespace Password_Generator.Controllers
{
    [Route("Password")]
    public class PasswordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PasswordController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Simple password generation
        [HttpPost("GeneratePassword")]
        public IActionResult GeneratePassword(int length, bool includeSpecialChars)
        {
            var password = GenerateRandomPassword(length, includeSpecialChars);
            return Json(new { success = true, password });
        }

        private string GenerateRandomPassword(int length, bool includeSpecialChars)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string specialChars = "!@#$%^&*()_+[]{}|;:,.<>?";

            var chars = validChars + (includeSpecialChars ? specialChars : string.Empty);
            var random = new Random();
            var password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        // Advanced password generation with detailed options
        [HttpPost("GenerateAdvancedPassword")]
        public IActionResult GenerateAdvancedPassword([FromBody] AdvancedPasswordRequest request)
        {
            try
            {
                var password = GenerateAdvancedPassword(
                    request.Length,
                    request.NumUpper,
                    request.NumLower,
                    request.NumDigits,
                    request.NumSpecial);
                return Json(new { success = true, password });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string GenerateAdvancedPassword(int length, int numUpper, int numLower, int numDigits, int numSpecial)
        {
            if (numUpper + numLower + numDigits + numSpecial > length)
                throw new ArgumentException("Sum of required characters exceeds total length.");

            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()_+[]{}|;:,.<>?";

            var random = new Random();
            var passwordChars = new List<char>();

            // Add required characters from each set
            for (int i = 0; i < numUpper; i++)
                passwordChars.Add(upperChars[random.Next(upperChars.Length)]);
            for (int i = 0; i < numLower; i++)
                passwordChars.Add(lowerChars[random.Next(lowerChars.Length)]);
            for (int i = 0; i < numDigits; i++)
                passwordChars.Add(digits[random.Next(digits.Length)]);
            for (int i = 0; i < numSpecial; i++)
                passwordChars.Add(specialChars[random.Next(specialChars.Length)]);

            // Fill the remaining characters with a mix of all sets
            string allChars = upperChars + lowerChars + digits + specialChars;
            for (int i = passwordChars.Count; i < length; i++)
                passwordChars.Add(allChars[random.Next(allChars.Length)]);

            // Shuffle to randomize character positions
            passwordChars = passwordChars.OrderBy(x => random.Next()).ToList();
            return new string(passwordChars.ToArray());
        }

        // DTO for advanced password generation
        public class AdvancedPasswordRequest
        {
            public int Length { get; set; }
            public int NumUpper { get; set; }
            public int NumLower { get; set; }
            public int NumDigits { get; set; }
            public int NumSpecial { get; set; }
        }

        public class PasswordHistoryDTO
        {
            public string OldPassword { get; set; }
            public DateTime CreatedAt { get; set; }
        }


        [HttpGet("GetPasswordHistory/{vendorName}")]
        public IActionResult GetPasswordHistory(string vendorName)
        {
            // Retrieve the logged-in user's email from the claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { success = false, message = "User not authenticated." });

            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // Find the vendor for this user by vendor name
            var vendor = _context.VendorPasswords.FirstOrDefault(v => v.UserId == user.Id && v.VendorName == vendorName);
            if (vendor == null)
                return Json(new { success = false, message = "Vendor not found." });

            // Retrieve all history records for this vendor, ordered by CreatedAt descending
            var history = _context.PasswordHistories
                .Where(ph => ph.VendorPasswordId == vendor.Id)
                .OrderByDescending(ph => ph.CreatedAt)
                .ToList();

            // Convert timestamps to Eastern Standard Time (EST)
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var historyDTOs = history.Select(ph => new PasswordHistoryDTO
            {
                OldPassword = ph.OldPassword,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(ph.CreatedAt, estZone)
            }).ToList();

            return Json(new { success = true, history = historyDTOs });
        }

        [HttpPost("AddPassword")]
        public IActionResult AddPassword([FromBody] VendorPassword model)
        {
            // Retrieve the logged-in user's email from the claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Find the corresponding user record
            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            model.UserId = user.Id;

            // Capitalize each word in VendorName
            model.VendorName = CapitalizeWords(model.VendorName);

            // Check if a vendor password for the same vendor already exists for this user
            var existingVendor = _context.VendorPasswords
                .FirstOrDefault(vp => vp.UserId == user.Id && vp.VendorName == model.VendorName);

            if (existingVendor != null)
            {
                // Record the current password to history before updating
                var history = new PasswordHistory
                {
                    OldPassword = existingVendor.CurrentPassword,
                    VendorPasswordId = existingVendor.Id
                };
                _context.PasswordHistories.Add(history);

                // Update the existing vendor record with the new password (and URL if needed)
                existingVendor.CurrentPassword = model.CurrentPassword ?? existingVendor.CurrentPassword;
                existingVendor.Url = model.Url;
                _context.VendorPasswords.Update(existingVendor);
            }
            else
            {
                // Ensure CurrentPassword is not null
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    return Json(new { success = false, message = "CurrentPassword cannot be null." });
                }

                // Add a new vendor password entry if none exists
                _context.VendorPasswords.Add(model);
            }

            _context.SaveChanges();
            return Json(new { success = true, message = "Password entry added." });
        }

        private string CapitalizeWords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        [HttpPost("DeletePassword")]
        public IActionResult DeletePassword([FromBody] int id)
        {
            // Retrieve the logged-in user's email from the claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Find the corresponding user record
            var user = _context.Password_Generator_Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Find the vendor password entry by id and user id
            var vendorPassword = _context.VendorPasswords.FirstOrDefault(vp => vp.Id == id && vp.UserId == user.Id);
            if (vendorPassword == null)
            {
                return Json(new { success = false, message = "Password entry not found." });
            }

            // Remove the vendor password entry
            _context.VendorPasswords.Remove(vendorPassword);
            _context.SaveChanges();

            return Json(new { success = true, message = "Password entry deleted." });
        }

    }
}

