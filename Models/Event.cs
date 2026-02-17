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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        
        // Navigation property
        public User? Organizer { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        //Date Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if event date is in the past
            if (EventDate.Date < DateTime.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "Event date cannot be in the past.",
                    new[] { nameof(EventDate) }
                );
            }

            // Check if start time is after end time
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    "Start time must be before end time.",
                    new[] { nameof(StartTime), nameof(EndTime) }
                );
            }

            // Check if event is at least 30 minutes long
            if ((EndTime - StartTime).TotalMinutes < 30)
            {
                yield return new ValidationResult(
                    "Event must be at least 30 minutes long.",
                    new[] { nameof(EndTime) }
                );
            }
        }
    }
}