namespace RaceDayWeb.Models
{
    public class EventListItem
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = "";
        public string? BannerUrl { get; set; }
        public string VenueName { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
        public string OrganiserName { get; set; } = "";
    }

    public class CategoryItem
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal DistanceKM { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
        public int EnrolledCount { get; set; }
    }

    public class EventDetail : EventListItem
    {
        public List<CategoryItem> Categories { get; set; } = new();
    }

    public class EventsIndexViewModel
    {
        public List<EventListItem> Events { get; set; } = new();
        public string? EventType { get; set; }
        public string? Search { get; set; }
        public bool UpcomingOnly { get; set; }
    }
}