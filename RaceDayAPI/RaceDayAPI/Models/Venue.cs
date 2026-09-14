using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }

        [Required]
        [MaxLength(150)]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Province { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        // Navigation property
        public ICollection<Event>? Events { get; set; }
    }
}