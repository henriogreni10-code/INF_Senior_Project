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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to use lowercase (PostgreSQL convention)
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Event>().ToTable("events");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<Vendor>().ToTable("vendors");
            modelBuilder.Entity<Message>().ToTable("messages");

            // Configure relationships
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Attendee)
                .WithMany()
                .HasForeignKey(b => b.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Vendor)
                .WithMany(v => v.Messages)
                .HasForeignKey(m => m.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Event)
                .WithMany()
                .HasForeignKey(m => m.EventId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}