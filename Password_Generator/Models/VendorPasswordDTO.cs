﻿namespace Password_Generator.Models
{
    public class VendorPasswordDTO
    {
        public int Id { get; set; }
        public string VendorName { get; set; }
        public string CurrentPassword { get; set; }
        public DateTime DateCreated { get; set; }
    }
}

