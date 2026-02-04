using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        public string UserType { get; set; } // "Organizer", "Vendor", "Attendee"
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}