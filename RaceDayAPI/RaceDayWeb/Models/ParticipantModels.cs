using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models
{
    public class MyEnrolment
    {
        public int EnrolmentID { get; set; }
        public string Status { get; set; } = "";
        public DateTime EnrolmentDate { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = "";
        public string CategoryName { get; set; } = "";
    }

    public class MyResult
    {
        public int ResultID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "";
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public int TotalFinishers { get; set; }

        public string PositionText
        {
            get
            {
                if (Status != "Finished" || Position == null) return Status == "DNF" ? "Did not finish" : "Did not start";
                return $"{Ordinal(Position.Value)} out of {TotalFinishers} finishers";
            }
        }

        private static string Ordinal(int n)
        {
            if (n % 100 is 11 or 12 or 13) return n + "th";
            return (n % 10) switch { 1 => n + "st", 2 => n + "nd", 3 => n + "rd", _ => n + "th" };
        }
    }

    public class ProfileData
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ProfileFormModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string Email { get; set; } = "";

        public string? ProfilePictureUrl { get; set; }
    }
}