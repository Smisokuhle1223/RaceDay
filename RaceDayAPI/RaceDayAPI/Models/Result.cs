using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        // Foreign key to Enrolment - UNIQUE enforces one-to-one
        [Required]
        public int EnrolmentID { get; set; }

        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Finished"; // "Finished", "DNF", "DNS"

        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}