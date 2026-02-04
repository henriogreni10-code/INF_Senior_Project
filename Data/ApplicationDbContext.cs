using Microsoft.EntityFrameworkCore;
using INF_SP.Models;

namespace INF_SP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}