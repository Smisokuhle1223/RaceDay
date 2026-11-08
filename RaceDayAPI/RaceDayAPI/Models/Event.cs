using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = "Run"; // "Run", "Walk" or "Cycle"

        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        // Foreign key to Venue
        [Required]
        public int VenueID { get; set; }

        [ForeignKey("VenueID")]
        public Venue? Venue { get; set; }

        // Foreign key to User (the Organiser)
        [Required]
        public int OrganiserID { get; set; }

        [ForeignKey("OrganiserID")]
        public User? Organiser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Category>? Categories { get; set; }
    }
}