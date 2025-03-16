using System.ComponentModel.DataAnnotations;

namespace Password_Generator.Models
{
    public class PasswordHistory
    {
        public int Id { get; set; }

        [Required]
        public string OldPassword { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to VendorPassword
        public int VendorPasswordId { get; set; }

        public VendorPassword VendorPassword { get; set; }

        // New date property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

