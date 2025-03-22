using Microsoft.EntityFrameworkCore;
using Password_Generator.Models;

namespace Password_Generator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User_Password_Generator> Password_Generator_Users { get; set; }
        public DbSet<VendorPassword> VendorPasswords { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
    }
}
