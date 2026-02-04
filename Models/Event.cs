using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required]
        public int OrganizerId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 10000)]
        public int Capacity { get; set; }
        
        public string Category { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public User? Organizer { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}