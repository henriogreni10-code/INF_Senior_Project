using System.ComponentModel.DataAnnotations;

namespace INF_SP.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public int AttendeeId { get; set; }
        
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        public string Status { get; set; } = "Confirmed";
        
        // Navigation properties
        public Event? Event { get; set; }
        public User? Attendee { get; set; }
    }
}