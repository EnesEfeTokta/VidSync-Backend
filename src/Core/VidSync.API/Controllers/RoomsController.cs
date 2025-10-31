using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidSync.Persistence;
using VidSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VidSync.API.DTOs;
using VidSync.Domain.Interfaces;

namespace VidSync.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationSummarizationService _summarizationService;

        public RoomsController(AppDbContext context, IConversationSummarizationService summarizationService)
        {
            _context = context;
            _summarizationService = summarizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            if (rooms == null || !rooms.Any())
            {
                ModelState.AddModelError("Room", "No rooms found.");
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
                ModelState.AddModelError("Room", "Room name is already in use.");
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

        [HttpGet("{roomId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetRoomMessages(Guid roomId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            const int maxPageSize = 100;
            pageSize = Math.Min(pageSize, maxPageSize);

            if (pageNumber <= 0 || pageSize <= 0)
            {
                ModelState.AddModelError("Pagination", "Page number and page size must be greater than zero.");
                return ValidationProblem(ModelState);
            }

            var messages = await _context.Messages
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    RoomId = m.RoomId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.MiddleName} {m.Sender.LastName}",
                    SentAt = m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }
        
        [HttpPost("{roomId}/summarize")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> SummarizeRoomContent(Guid roomId)
        {
            try
            {
                await _summarizationService.SummarizeAndSaveAsync(roomId);
                
                return Accepted(new { message = "Summarization process has been initiated." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "The AI service is currently unavailable.", details = ex.Message });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}