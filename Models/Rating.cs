using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class Rating
    {
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }
        
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Event? Event { get; set; }
        public User? User { get; set; }
    }
}