using Domain.Enum;
using System.Text.Json.Serialization;

namespace Application.DTOs
{
    public record BusinessCardDto(
   Guid Id,
   string Name,
   Gender Gender,
   DateTime? DateOfBirth,
   string Email,
   string Phone,
   string Address,
   string? PhotoBase64,
   int? PhotoSizeBytes
   );
    public record CreateBusinessCardRequest(
  [property: JsonIgnore] Guid? Id,
string Name,
Gender Gender,
DateTime? DateOfBirth,
string Email,
string Phone,
string Address,
string? PhotoBase64,
int? PhotoSizeBytes
);
}

