using Microsoft.EntityFrameworkCore;
using VidSync.Domain.DTOs.AiSummaryChat.Request;
using VidSync.Domain.DTOs.AiSummaryChat.Response;
using VidSync.Domain.Interfaces;

namespace VidSync.Persistence.Services
{
    public class ConversationSummarizationService : IConversationSummarizationService
    {
        private readonly AppDbContext _context;
        private readonly IAiServiceClient _aiServiceClient;
        private readonly ICryptoService _cryptoService;
        private readonly IEmailService _emailService;

        public ConversationSummarizationService(
            AppDbContext context,
            IAiServiceClient aiServiceClient,
            ICryptoService cryptoService,
            IEmailService emailService)
        {
            _context = context;
            _aiServiceClient = aiServiceClient;
            _cryptoService = cryptoService;
            _emailService = emailService;
        }

        public async Task SummarizeAndSaveAsync(Guid roomId)
        {
            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");
            }

            var messages = await _context.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .AsNoTracking()
                .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            var participants = messages.Select(m => m.Sender).Distinct().ToList();

            var payload = new ConversationPayloadRequest
            {
                Metadata = new MetadataRequestDto
                {
                    ChatId = room.Id.ToString(),
                    Title = room.Name,
                    StartTimeUtc = room.CreatedAt,
                    EndTimeUtc = DateTime.UtcNow,
                    Language = "tr-TR"
                },
                Participants = participants.Select(p => new ParticipantRequestDto
                {
                    ParticipantId = p.Id.ToString(),
                    FullName = $"{p.FirstName} {p.LastName}".Trim(),
                    Email = p.Email ?? string.Empty,
                    Role = "Participant"
                }).ToList(),
                ChatHistory = messages.Select((msg, index) => new ChatMessageRequestDto
                {
                    MessageId = index + 1,
                    SenderId = msg.SenderId.ToString(),
                    Message = msg.Content,
                    Timestamp = msg.SentAt.ToString("HH:mm:ss")
                }).ToList()
            };

            ConversationSummaryResponse summaryResponse = await _aiServiceClient.GetSummaryAsync(payload);

            var summaryText = summaryResponse.Summary;

            if (!string.IsNullOrEmpty(summaryText))
            {
                var encryptedSummary = _cryptoService.Encrypt(summaryText);

                var roomToUpdate = await _context.Rooms.FindAsync(roomId);
                if (roomToUpdate != null)
                {
                    roomToUpdate.Summary = encryptedSummary;
                    await _context.SaveChangesAsync();
                }

                await SendSummaryEmailsToParticipantsAsync(summaryResponse, room.Name);
            }
        }

        private async Task SendSummaryEmailsToParticipantsAsync(ConversationSummaryResponse summaryResponse, string roomName)
        {
            if (summaryResponse?.Participants == null || !summaryResponse.Participants.Any())
            {
                return;
            }

            var subject = $"Toplantı Özeti: {roomName}";

            var distinctParticipants = summaryResponse.Participants
                .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                .GroupBy(p => p.Email)
                .Select(g => g.First());

            foreach (var participant in distinctParticipants)
            {
                try
                {
                    var body = $@"
                <html>
                <body>
                    <h3>Merhaba {participant.FullName},</h3>
                    <p>'{roomName}' başlıklı toplantının özeti aşağıdadır:</p>
                    <hr>
                    <p><i>{summaryResponse.Summary.Replace("\n", "<br>")}</i></p>
                    <hr>
                    <p>İyi günler dileriz,<br>VidSync Ekibi</p>
                </body>
                </html>";

                    await _emailService.SendEmailAsync(participant.Email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"E-posta gönderilemedi: {participant.Email}. Hata: {ex.Message}");
                }
            }
        }
    }
}