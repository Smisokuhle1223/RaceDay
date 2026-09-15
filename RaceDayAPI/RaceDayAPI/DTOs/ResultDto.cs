using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class ResultDto
    {
        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Finished"; // "Finished", "DNF", "DNS"
    }
}