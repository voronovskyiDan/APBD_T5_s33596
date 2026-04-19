using System.ComponentModel.DataAnnotations;

namespace APBD_T5_s33596.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; } = String.Empty;

        [Required]
        [MinLength(1)]
        public string BuildingCode { get; set; } = String.Empty;
        public int Floor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive integer.")]
        public int Capacity { get; set; }
        public bool HasProjector { get; set; }
        public bool IsActive { get; set; }

    }
}
