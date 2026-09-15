using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKM { get; set; }

        public int MaxParticipants { get; set; } = 500;

        public decimal EntryFee { get; set; } = 0;
    }
}