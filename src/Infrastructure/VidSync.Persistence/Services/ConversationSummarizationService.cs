using Microsoft.EntityFrameworkCore;
using VidSync.Domain.DTOs.AiSummaryChat.Request;
using VidSync.Domain.Interfaces;

namespace VidSync.Persistence.Services;

public class ConversationSummarizationService : IConversationSummarizationService
{
    private readonly AppDbContext _context;
    private readonly IAiServiceClient _aiServiceClient;
    private readonly ICryptoService _cryptoService;

    public ConversationSummarizationService(AppDbContext context, IAiServiceClient aiServiceClient, ICryptoService cryptoService)
    {
        _context = context;
        _aiServiceClient = aiServiceClient;
        _cryptoService = cryptoService;
    }

    public async Task SummarizeAndSaveAsync(Guid roomId)
    {
        // 1. Gerekli verileri veritabanından çek
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {roomId} not found.");
        }

        var messages = await _context.Messages
            .Where(m => m.RoomId == roomId)
            .Include(m => m.Sender) // Katılımcı bilgilerini almak için Sender'ı dahil et
            .OrderBy(m => m.SentAt)
            .AsNoTracking()
            .ToListAsync();

        // Mesajlardan benzersiz katılımcıları al
        var participants = messages.Select(m => m.Sender).Distinct().ToList();

        // 2. Veriyi AI servisinin beklediği DTO formatına dönüştür (Mapping)
        var payload = new ConversationPayloadRequest
        {
            Metadata = new MetadataRequestDto
            {
                ChatId = room.Id.ToString(),
                Title = room.Name,
                StartTimeUtc = room.CreatedAt, // Gerçek bir session başlangıç zamanı kullanmak daha iyi olabilir
                EndTimeUtc = DateTime.UtcNow, // İşlemin yapıldığı an
                Language = "tr-TR"
            },
            Participants = participants.Select(p => new ParticipantRequestDto
            {
                ParticipantId = p.Id.ToString(),
                FullName = $"{p.FirstName} {p.LastName}".Trim(),
                Email = p.Email,
                Role = "Participant" // Rolü veritabanından da alabilirsin
            }).ToList(),
            ChatHistory = messages.Select((msg, index) => new ChatMessageRequestDto
            {
                MessageId = index + 1, // Basit bir ID ataması
                SenderId = msg.SenderId.ToString(),
                Message = msg.Content,
                Timestamp = msg.SentAt.ToString("HH:mm:ss")
            }).ToList(),
            ProcessingResults = new ProcessingResultsRequestDto() // İstek için boş gönderiyoruz
        };

        // 3. AI servisini çağır ve yanıtı al
        var summaryResponse = await _aiServiceClient.GetSummaryAsync(payload);
        var summaryText = summaryResponse.ProcessingResults.Summary.Overall;

        // 4. Yanıtı işle ve veritabanına kaydet
        if (!string.IsNullOrEmpty(summaryText))
        {
            var encryptedSummary = _cryptoService.Encrypt(summaryText);

            // Güncelleme için odayı tekrar context'e al
            var roomToUpdate = await _context.Rooms.FindAsync(roomId);
            if (roomToUpdate != null)
            {
                // ÖNEMLİ: 'Room' entity sınıfına 'public string EncryptedSummary { get; set; }'
                // gibi bir alan eklemen ve bir migration oluşturman gerekecek.
                // roomToUpdate.EncryptedSummary = encryptedSummary; 
                await _context.SaveChangesAsync();
            }
        }

        // TODO: Gelecekte, summaryResponse'dan gelen Action Items ve Scheduled Events'i de
        // veritabanındaki ilgili tablolara kaydedebilirsin.
    }
}
