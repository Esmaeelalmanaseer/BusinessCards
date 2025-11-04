using Domain.Enum;

namespace Domain.Entities;

public class BusinessCard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Male;
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PhotoBase64 { get; set; }
    public int? PhotoSizeBytes { get; set; }
}
