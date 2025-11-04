using Application.DTOs;
using Application.Service;
using Azure.Core;
using Domain.Helper;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessCardsController : ControllerBase
    {
        private readonly IBusinessCardService _service;

        public BusinessCardsController(IBusinessCardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<Pagination<BusinessCardDto>>>> GetAll([FromQuery] BusinessCardParams p)
        {
            var response = await _service.GetAll(p);
            return Ok(response);
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BusinessCardDto>>> Create([FromBody] CreateBusinessCardRequest req)
        {
            const int MaxPhotoBytes = 1_048_576;
            if (req.PhotoSizeBytes is not null && req.PhotoSizeBytes > MaxPhotoBytes)
            {
                return BadRequest(ApiResponse<BusinessCardDto>.FailureResponse("Photo exceeds 1MB limit."));
            }
            var response = await _service.Save(req);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BusinessCardDto>> GetById(Guid id)
        {
            var response = await _service.GetById(id);
            if (!response.Success)
                return NotFound(response);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _service.Delete(id);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
        [HttpPost("import/csv")]
        public async Task<ActionResult<ApiResponse<int>>> ImportCsv([FromForm] IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResponse<int>.FailureResponse("CSV file is required."));

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvHelper.CsvReader(stream, System.Globalization.CultureInfo.InvariantCulture);

            var records = new List<CreateBusinessCardRequest>();
            try
            {
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    var name = csv.GetField("Name") ?? "";
                    var genderStr = csv.GetField("Gender") ?? "Male";
                    var dobStr = csv.GetField("DateOfBirth");
                    var email = csv.GetField("Email") ?? "";
                    var phone = csv.GetField("Phone") ?? "";
                    var address = csv.GetField("Address") ?? "";
                    var photoBase64 = csv.GetField("PhotoBase64");
                    var photoSizeStr = csv.GetField("PhotoSizeBytes");

                    Domain.Enum.Gender gender = Enum.TryParse<Domain.Enum.Gender>(genderStr, true, out var g) ? g : Domain.Enum.Gender.Male;
                    DateTime? dob = DateTime.TryParse(dobStr, out var d) ? d : null;
                    int? photoSize = int.TryParse(photoSizeStr, out var ps) ? ps : null;

                    if (photoSize is not null && photoSize > 1_048_576)
                        return BadRequest(ApiResponse<int>.FailureResponse($"Row with Name='{name}' exceeds 1MB photo limit."));

                    records.Add(new CreateBusinessCardRequest(
                       Guid.Empty, name, gender, dob, email, phone, address, photoBase64, photoSize
                    ));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.FailureResponse($"Invalid CSV format. {ex.Message}"));
            }

            var result = await _service.SaveRange(records);
            if (result.Success)
                return Ok(ApiResponse<int>.SuccessResponse(records.Count(), $"Imported {records.Count()} records."));
            return BadRequest("Error Saved");
        }
        [HttpPost("import/xml")]
        public async Task<ActionResult<ApiResponse<int>>> ImportXml([FromForm] IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResponse<int>.FailureResponse("XML file is required."));

            using var stream = file.OpenReadStream();
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(BusinessCardXmlEnvelope));
                if (serializer.Deserialize(stream) is not BusinessCardXmlEnvelope env || env.Items is null)
                    return BadRequest(ApiResponse<int>.FailureResponse("XML payload is empty or invalid."));

                List<CreateBusinessCardRequest> createBusinessCardRequests = [];
                foreach (var x in env.Items ?? [])
                {
                    int? size = null;
                    if (!string.IsNullOrWhiteSpace(x.PhotoSizeBytes) && int.TryParse(x.PhotoSizeBytes, out var s)) size = s;
                    if (size is not null && size > 1_048_576)
                        return BadRequest(ApiResponse<int>.FailureResponse($"Item '{x.Name}' exceeds 1MB photo limit."));

                    DateTime? dob = null;
                    if (!string.IsNullOrWhiteSpace(x.DateOfBirth) && DateTime.TryParse(x.DateOfBirth, out var d)) dob = d;
                    var gender = Enum.TryParse<Domain.Enum.Gender>(x.Gender, true, out var g) ? g : Domain.Enum.Gender.Male;

                    var req = new CreateBusinessCardRequest(

                       Guid.Empty, x.Name ?? "", gender, dob, x.Email ?? "", x.Phone ?? "", x.Address ?? "", x.PhotoBase64, size
                    );

                    createBusinessCardRequests.Add(req);
                }
                var result = await _service.SaveRange(createBusinessCardRequests);
                if (result.Success)
                    return Ok(ApiResponse<int>.SuccessResponse(createBusinessCardRequests.Count(), $"Imported {createBusinessCardRequests.Count()} records."));
                return BadRequest("Error Saved");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.FailureResponse($"Invalid XML format. {ex.Message}"));
            }
        }
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var all = await _service.GetAll(new BusinessCardParams { PageNumber = 1, pageSize = int.MaxValue });

            using var mem = new MemoryStream();
            using (var writer = new StreamWriter(mem, leaveOpen: true))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteField("Id"); csv.WriteField("Name"); csv.WriteField("Gender"); csv.WriteField("DateOfBirth");
                csv.WriteField("Email"); csv.WriteField("Phone"); csv.WriteField("Address"); csv.WriteField("PhotoBase64"); csv.WriteField("PhotoSizeBytes");
                await csv.NextRecordAsync();

                foreach (var item in all.Data.Data)
                {
                    csv.WriteField(item.Id);
                    csv.WriteField(item.Name);
                    csv.WriteField(item.Gender.ToString());
                    csv.WriteField(item.DateOfBirth?.ToString("yyyy-MM-dd"));
                    csv.WriteField(item.Email);
                    csv.WriteField(item.Phone);
                    csv.WriteField(item.Address);
                    csv.WriteField(item.PhotoBase64);
                    csv.WriteField(item.PhotoSizeBytes?.ToString());
                    await csv.NextRecordAsync();
                }
                await writer.FlushAsync();
            }
            mem.Position = 0;
            return File(mem.ToArray(), "text/csv", $"business_cards_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
        [HttpGet("export/xml")]
        public async Task<IActionResult> ExportXml()
        {
            var all = await _service.GetAll(new BusinessCardParams { PageNumber = 1, pageSize = int.MaxValue });

            var env = new BusinessCardXmlEnvelope
            {
                Items = all.Data.Data.Select(x => new BusinessCardXmlItem
                {
                    Name = x.Name,
                    Gender = x.Gender.ToString(),
                    DateOfBirth = x.DateOfBirth?.ToString("yyyy-MM-dd"),
                    Email = x.Email,
                    Phone = x.Phone,
                    Address = x.Address,
                    PhotoBase64 = x.PhotoBase64,
                    PhotoSizeBytes = x.PhotoSizeBytes?.ToString()
                }).ToList()
            };

            using var mem = new MemoryStream();
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(BusinessCardXmlEnvelope));
            serializer.Serialize(mem, env);
            mem.Position = 0;
            return File(mem.ToArray(), "application/xml", $"business_cards_{DateTime.UtcNow:yyyyMMddHHmmss}.xml");
        }
    }
}
