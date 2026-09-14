using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        // Foreign key to User (the Participant)
        [Required]
        public int ParticipantID { get; set; }

        [ForeignKey("ParticipantID")]
        public User? Participant { get; set; }

        // Foreign key to Category
        [Required]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed"; // "Pending", "Confirmed", "Cancelled"

        // Navigation property
        public Result? Result { get; set; }
    }
}