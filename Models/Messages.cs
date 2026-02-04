using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        [Required]
        public int SenderId { get; set; }
        
        [Required]
        public int RecipientId { get; set; }
        
        public int? VendorId { get; set; }
        
        public int? EventId { get; set; }
        
        [Required]
        public string MessageText { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        // Navigation properties
        public User? Sender { get; set; }
        public User? Recipient { get; set; }
        public Vendor? Vendor { get; set; }
        public Event? Event { get; set; }
    }
}