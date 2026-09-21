using System.ComponentModel.DataAnnotations;

namespace Ping.Infrastructure.Configuration
{
    public class DatabaseConfiguration
    {
        public const string SectionName = "Database";

        [Required]
        public required string ConnectionString { get; set; }
    }
}
