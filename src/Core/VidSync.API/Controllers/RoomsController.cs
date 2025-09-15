using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidSync.Persistence;
using VidSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VidSync.API.DTOs;

namespace VidSync.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            if (rooms == null || !rooms.Any())
            {
                ModelState.AddModelError("Room", "Room is already in use.");
                return ValidationProblem(ModelState);
            }
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                ModelState.AddModelError("Room", "Room not found.");
                return ValidationProblem(ModelState);
            }

            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Name.ToLower() == createRoomDto.Name.ToLower());
            if (existingRoom != null)
            {
                ModelState.AddModelError("Room", "Room is already in use.");
                return ValidationProblem(ModelState);
            }

            var room = new Room
            {
                Name = createRoomDto.Name,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = createRoomDto.ExpiresAt,
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

           return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }
    }
}
