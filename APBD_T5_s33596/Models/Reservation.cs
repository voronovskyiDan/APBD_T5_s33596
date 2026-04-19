using APBD_T5_s33596.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace APBD_T5_s33596.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int RoomId { get; set; }

        [MinLength(1)]
        public string OrganizerName { get; set; } = string.Empty;

        [MinLength(1)]
        public string Topic { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
