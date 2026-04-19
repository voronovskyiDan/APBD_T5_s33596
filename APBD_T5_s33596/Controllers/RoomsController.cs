using APBD_T5_s33596.Database;
using APBD_T5_s33596.Models;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;

namespace APBD_T5_s33596.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? minCapacity = null,
            [FromQuery] bool? hasProjector = null
            )
        {
            var rooms = DataStore.Rooms.AsEnumerable();
            if (minCapacity.HasValue)
            {
                rooms = rooms.Where(r => r.Capacity >= minCapacity.Value);
            }
            if (hasProjector.HasValue)
            {
                rooms = rooms.Where(r => r.HasProjector == hasProjector.Value);
            }

            return Ok(rooms.ToList());
        }


        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound($"Room with ID {id} not found.");
            }
            return Ok(room);
        }

        [HttpGet("building/{buildingCode}")]
        public IActionResult GetByBuildingCode(string buildingCode)
        {
            var rooms = DataStore.Rooms.Where(r => r.BuildingCode == buildingCode).ToList();
            if (!rooms.Any())
            {
                return NotFound($"No rooms found for building code {buildingCode}.");
            }
            return Ok(rooms);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Room room)
        {
            room.Id = DataStore.NextRoomId;
            DataStore.Rooms.Add(room);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Room room)
        {
            var existingRoom = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (existingRoom == null)
            {
                return NotFound($"Room with ID {id} not found.");
            }
            existingRoom.Name = room.Name;
            existingRoom.BuildingCode = room.BuildingCode;
            existingRoom.Capacity = room.Capacity;
            existingRoom.HasProjector = room.HasProjector;
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NoContent();
            }

            if(DataStore.Reservations.Any(r => r.RoomId == id && r.Date >= DateOnly.FromDateTime(DateTime.Now)))
            {
                return Conflict("Cannot delete a room with upcoming reservations.");
            }

            DataStore.Rooms.Remove(room);
            return NoContent();
        }
    }
}
