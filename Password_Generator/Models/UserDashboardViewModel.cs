namespace Password_Generator.Models
{
    public class UserDashboardViewModel
    {
        public User_Password_Generator User { get; set; }
        public List<VendorPasswordDTO> VendorPasswords { get; set; } = new List<VendorPasswordDTO>();
    }
}


