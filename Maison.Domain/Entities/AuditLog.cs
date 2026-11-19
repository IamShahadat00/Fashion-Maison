using System;

namespace Maison.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public Guid? ActorId { get; set; }
        public string ActorEmail { get; set; } = "System";
        public string ActorRole { get; set; } = "System";
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}