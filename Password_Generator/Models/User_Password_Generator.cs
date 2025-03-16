using System.ComponentModel.DataAnnotations;

namespace Password_Generator.Models
{
    public class User_Password_Generator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public SecurityQuestion ResetQuestion1 { get; set; }

        [Required]
        public string ResetAnswer1 { get; set; }

        [Required]
        public SecurityQuestion ResetQuestion2 { get; set; }

        [Required]
        public string ResetAnswer2 { get; set; }
    }
}


