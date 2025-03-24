using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Password_Generator.Models
{
    public enum PasswordCategory
    {
        Social_Media,
        Email,
        Banking,
        Shopping,
        Entertainment,
        Work,
        Education,
        Health,
        Travel,
        Utilities,
        Gaming,
        Finance,
        Government,
        Communication,
        Other
    }

    public class VendorPassword
    {
        public int Id { get; set; }

        [Required]
        public string VendorName { get; set; }

        [Url]
        public string Url { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        // Link to the user (assumes your User_Password_Generator model has an Id property)
        public int UserId { get; set; }
        public User_Password_Generator User { get; set; }

        // Navigation for password history entries
        public List<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

        // New property for when the vendor record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // New property for password category
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public PasswordCategory Category { get; set; }
        public string Username { get; set; }
    }
}

