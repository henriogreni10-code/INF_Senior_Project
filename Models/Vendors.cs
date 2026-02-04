using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string BusinessName { get; set; } = string.Empty;
        
        [Required]
        public string ServiceType { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string PriceRange { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public User? User { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}