namespace Zadanie5.Controllers;

using Microsoft.AspNetCore.Mvc;
using APBD_Zadanie_6.Data;
using APBD_Zadanie_6.Models;


[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> GetReservations([FromQuery] DateOnly? date, [FromQuery] string? status, [FromQuery] int? roomId)
    {
        var query = DataStore.Reservations.AsQueryable();

        if (date.HasValue)
            query = query.Where(r => r.Date == date.Value);
            
        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            
        if (roomId.HasValue)
            query = query.Where(r => r.RoomId == roomId.Value);

        return Ok(query.ToList());
    }

    [HttpGet("{id}")]
    public ActionResult<Reservation> GetReservation(int id)
    {
        var res = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpPost]
    public ActionResult<Reservation> CreateReservation([FromBody] Reservation newRes)
    {
        var targetRoom = DataStore.Rooms.FirstOrDefault(r => r.Id == newRes.RoomId);
        
        if (targetRoom == null) return NotFound("The requested room does not exist.");
        if (!targetRoom.IsActive) return BadRequest("Cannot reserve an inactive room.");

        if (IsOverlapping(newRes))
            return Conflict("The room is already reserved during this time.");

        newRes.Id = DataStore.Reservations.Max(r => r.Id) + 1;
        DataStore.Reservations.Add(newRes);

        return CreatedAtAction(nameof(GetReservation), new { id = newRes.Id }, newRes);
    }

    [HttpPut("{id}")]
    public ActionResult UpdateReservation(int id, [FromBody] Reservation updatedRes)
    {
        var existingRes = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
        if (existingRes == null) return NotFound();

        var targetRoom = DataStore.Rooms.FirstOrDefault(r => r.Id == updatedRes.RoomId);
        if (targetRoom == null) return NotFound("The requested room does not exist.");
        if (!targetRoom.IsActive) return BadRequest("Cannot reserve an inactive room.");

        var tempReservations = DataStore.Reservations.Where(r => r.Id != id).ToList();
        
        bool overlaps = tempReservations.Any(r => 
            r.RoomId == updatedRes.RoomId && 
            r.Date == updatedRes.Date &&
            updatedRes.StartTime < r.EndTime && 
            updatedRes.EndTime > r.StartTime);

        if (overlaps) return Conflict("The updated time conflicts with an existing reservation.");

        existingRes.RoomId = updatedRes.RoomId;
        existingRes.OrganizerName = updatedRes.OrganizerName;
        existingRes.Topic = updatedRes.Topic;
        existingRes.Date = updatedRes.Date;
        existingRes.StartTime = updatedRes.StartTime;
        existingRes.EndTime = updatedRes.EndTime;
        existingRes.Status = updatedRes.Status;

        return Ok(existingRes);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteReservation(int id)
    {
        var res = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
        if (res == null) return NotFound();

        DataStore.Reservations.Remove(res);
        return NoContent();
    }

    private bool IsOverlapping(Reservation newRes)
    {
        return DataStore.Reservations.Any(r => 
            r.RoomId == newRes.RoomId && 
            r.Date == newRes.Date &&
            newRes.StartTime < r.EndTime && 
            newRes.EndTime > r.StartTime);
    }
}