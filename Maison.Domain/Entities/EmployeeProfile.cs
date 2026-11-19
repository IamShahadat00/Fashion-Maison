using System;

namespace Maison.Domain.Entities
{
    public class EmployeeProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public decimal PerformanceScore { get; set; } // 0.0 - 100.0
        public string? ReviewNotes { get; set; }
        public DateTime HiredAt { get; set; } = DateTime.UtcNow;
        public DateTime LastReviewedAt { get; set; } = DateTime.UtcNow;
    }
}