using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CreateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = "Run";
    }

    public class UpdateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = "Run";
    }
}