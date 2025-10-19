using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VidSync.Domain.Interfaces;

namespace VidSync.Persistence.Converters;

public class EncryptedStringConverter : ValueConverter<string, string>
{
        public EncryptedStringConverter(ICryptoService cryptoService, ConverterMappingHints? mappingHints = null)
            : base(v => cryptoService.Encrypt(v), v => cryptoService.Decrypt(v), mappingHints)
        {
        }
}
