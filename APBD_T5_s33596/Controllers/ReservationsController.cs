using APBD_T5_s33596.Controllers.Common;
using APBD_T5_s33596.Database;
using APBD_T5_s33596.Models;
using APBD_T5_s33596.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APBD_T5_s33596.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] ReservationStatus? status,
            [FromQuery] int? roomId
            )
        {
            var reservations = DataStore.Reservations.AsEnumerable();

            if (date.HasValue)
            {
                reservations = reservations.Where(r => r.Date == date.Value);
            }
            if (status.HasValue)
            {
                reservations = reservations.Where(r => r.Status == status.Value);
            }
            if (roomId.HasValue)
            {
                reservations = reservations.Where(r => r.RoomId == roomId.Value);
            }

            return Ok(reservations.ToList());
        }
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var reservation = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            return Ok(reservation);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Reservation reservation)
        {
            var result = ValidateReservation(reservation);

            if (!result.IsValid)
                return ErrorResponse(result);

            reservation.Id = DataStore.NextReservationId;
            DataStore.Reservations.Add(reservation);
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Reservation reservation)
        {
            var existing = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (existing == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            var result = ValidateReservation(reservation, id);
            if (!result.IsValid)
                return ErrorResponse(result);


            existing.RoomId = reservation.RoomId;
            existing.OrganizerName = reservation.OrganizerName;
            existing.Topic = reservation.Topic;
            existing.Date = reservation.Date;
            existing.StartTime = reservation.StartTime;
            existing.EndTime = reservation.EndTime;
            existing.Status = reservation.Status;
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var reservation = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            DataStore.Reservations.Remove(reservation);
            return NoContent();
        }

        //General bussiness logic that why it is putted in one place and can be used in both Create and Update methods
        private ValidationRes ValidateReservation(Reservation reservation, int? ignoreReservationId = null) 
        {
            // Basically not busiiness but in case that I dont need to saparete business logic(Domain) and validation logic(Controlles) I put it in one method 
            if (reservation.EndTime <= reservation.StartTime)
            {
                return new ValidationRes
                {
                    IsValid = false,
                    ErrorMessage = "EndTime must be later than StartTime.",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == reservation.RoomId);
            if (room == null)
            {
                return new ValidationRes
                {
                    IsValid = false,
                    ErrorMessage = $"Room with ID {reservation.RoomId} does not exist.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            if (!room.IsActive)
            {
                return new ValidationRes
                {
                    IsValid = false,
                    ErrorMessage = $"Room with ID {reservation.RoomId} is not active.",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            if (IsOverlapping(reservation))
            {
                return new ValidationRes
                {
                    IsValid = false,
                    ErrorMessage = "The reservation overlaps with an existing reservation for the same room.",
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            return new ValidationRes { IsValid = true };
        }
        private bool IsOverlapping(Reservation newReservation)
        {
            return DataStore.Reservations.Any(r =>
                r.RoomId == newReservation.RoomId &&
                r.Date == newReservation.Date &&
                newReservation.StartTime < r.EndTime &&
                newReservation.EndTime > r.StartTime
            );
        }

        private IActionResult ErrorResponse(ValidationRes result)
        {
            return result.StatusCode switch
            {
                StatusCodes.Status400BadRequest => BadRequest(result.ErrorMessage),
                StatusCodes.Status404NotFound => NotFound(result.ErrorMessage),
                StatusCodes.Status409Conflict => Conflict(result.ErrorMessage),
                _ => BadRequest(result.ErrorMessage)
            };
        }

        

    }
}
