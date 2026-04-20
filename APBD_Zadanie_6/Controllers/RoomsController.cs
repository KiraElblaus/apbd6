using Microsoft.AspNetCore.Mvc;
using APBD_Zadanie_6.Data;
using APBD_Zadanie_6.Models;

namespace Zadanie5.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Room>> GetRooms([FromQuery] int? minCapacity, [FromQuery] bool? hasProjector,
        [FromQuery] bool? activeOnly)
    {
        var query = DataStore.Rooms.AsQueryable();

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        if (hasProjector.HasValue)
            query = query.Where(r => r.HasProjector == hasProjector.Value);
        if (activeOnly.HasValue && activeOnly.Value)
            query = query.Where(r => r.IsActive);

        return Ok(query.ToList());
    }

    [HttpGet("{id}")]
    public ActionResult<Room> GetRoom(int id)
    {
        var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
        if (room == null)
            return NotFound();
        return Ok(room);
    }
    
    [HttpGet("building/{buildingCode}")]
    public ActionResult<IEnumerable<Room>> GetRoomsByBuilding(string buildingCode)
    {
        var rooms = DataStore.Rooms
            .Where(r => r.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
            
        return Ok(rooms);
    }
    
    [HttpPost]
    public ActionResult<Room> CreateRoom([FromBody] Room newRoom)
    {
        newRoom.Id = DataStore.Rooms.Max(r => r.Id) + 1;
        DataStore.Rooms.Add(newRoom);

        return CreatedAtAction(nameof(GetRoom), new { id = newRoom.Id }, newRoom);
    }
    
    [HttpPut("{id}")]
    public ActionResult UpdateRoom(int id, [FromBody] Room updatedRoom)
    {
        var existingRoom = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
        if (existingRoom == null) return NotFound();

        existingRoom.Name = updatedRoom.Name;
        existingRoom.BuildingCode = updatedRoom.BuildingCode;
        existingRoom.Floor = updatedRoom.Floor;
        existingRoom.Capacity = updatedRoom.Capacity;
        existingRoom.HasProjector = updatedRoom.HasProjector;
        existingRoom.IsActive = updatedRoom.IsActive;

        return Ok(existingRoom);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteRoom(int id)
    {
        var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
        if (room == null) return NotFound();

        bool hasReservations = DataStore.Reservations.Any(res => res.RoomId == id);
        if (hasReservations)
            return Conflict("Cannot delete room because it has existing reservations.");

        DataStore.Rooms.Remove(room);
        return NoContent();
    }
}