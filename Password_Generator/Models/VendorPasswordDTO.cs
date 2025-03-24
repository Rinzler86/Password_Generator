namespace Password_Generator.Models
{
    public class VendorPasswordDTO
    {
        public int Id { get; set; }
        public string VendorName { get; set; }
        public string CurrentPassword { get; set; }
        public DateTime DateCreated { get; set; }
        public PasswordCategory Category { get; set; } // New property for category
        public string Username { get; set; }
        public string Url { get; set; }
    }
}


