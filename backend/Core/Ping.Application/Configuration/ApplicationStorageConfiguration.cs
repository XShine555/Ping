using System.ComponentModel.DataAnnotations;

namespace Ping.Application.Configuration
{
    public class ApplicationStorageConfiguration
    {
        public const string SectionName = "ApplicationStorage";

        [Required]
        public required string Bucket { get; set; }
    }
}
