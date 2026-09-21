using System.ComponentModel.DataAnnotations;

namespace Ping.Infrastructure.Configuration
{
    public class LiveKitConfiguration
    {
        public const string SectionName = "LiveKit";

        [Required]
        public required string ApiKey { get; set; }

        [Required]
        public required string ApiSecret { get; set; }

        public int TokenTtlMinutes { get; set; } = 240;
    }
}
