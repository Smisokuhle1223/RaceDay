using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        // Foreign key to Event
        [Required]
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKM { get; set; }

        public int MaxParticipants { get; set; } = 500;

        [Column(TypeName = "decimal(8,2)")]
        public decimal EntryFee { get; set; } = 0;

        // Navigation property
        public ICollection<Enrolment>? Enrolments { get; set; }
    }
}