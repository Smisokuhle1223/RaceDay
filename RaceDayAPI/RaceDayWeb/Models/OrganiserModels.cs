using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models
{
    public class DashboardEvent
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = "";
        public string? BannerUrl { get; set; }
        public string VenueName { get; set; } = "";
        public int CategoryCount { get; set; }
        public int TotalEnrolments { get; set; }
    }

    public class DashboardData
    {
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<DashboardEvent> UpcomingEvents { get; set; } = new();
        public List<DashboardEvent> Events { get; set; } = new();
    }

    public class VenueItem
    {
        public int VenueID { get; set; }
        public string VenueName { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
    }

    public class EventFormModel
    {
        public int? EventID { get; set; }

        [Required(ErrorMessage = "Please enter an event name.")]
        [MaxLength(150)]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose a date and time.")]
        [Display(Name = "Date and time")]
        public DateTime EventDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please choose a venue.")]
        [Display(Name = "Location")]
        public int VenueID { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }

        public string? ExistingBannerUrl { get; set; }
    }

    public class CategoryFormModel
    {
        public int EventID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = "";

        [Range(0.1, 1000)]
        public decimal DistanceKM { get; set; }

        [Range(1, 100000)]
        public int MaxParticipants { get; set; } = 100;

        [Range(0, 100000)]
        public decimal EntryFee { get; set; }
    }

    public class EnrolmentRow
    {
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime EnrolmentDate { get; set; }
        public int? ResultID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? ResultStatus { get; set; }
    }

    public class ResultFormModel
    {
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public int? ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "Finished";
    }
}