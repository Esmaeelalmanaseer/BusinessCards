using System.Net;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class ImportCsvXmlTests : IClassFixture<TestingWebAppFactory>
{
    private readonly TestingWebAppFactory _factory;
    public ImportCsvXmlTests(TestingWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task ImportCsv_Should_Create_Two()
    {
        var client = _factory.CreateClient();
        var csv = """
Name,Gender,DateOfBirth,Email,Phone,Address,PhotoBase64,PhotoSizeBytes
Alice Doe,Female,1993-02-11,alice@example.com,+962700000001,Amman,,0
Bob Smith,Male,1988-09-05,bob@example.com,+962700000002,Irbid,,0
""";
        var content = Encoding.UTF8.GetBytes(csv).AsMultipartFile("file", "sample.csv", "text/csv");
        var resp = await client.PostAsync("/api/BusinessCards/import/csv", content);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("Imported 2");
    }

    [Fact]
    public async Task ImportXml_Should_Create_Two()
    {
        var client = _factory.CreateClient();
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<BusinessCards>
  <BusinessCard>
    <Name>Charlie Brown</Name>
    <Gender>Male</Gender>
    <DateOfBirth>1990-07-01</DateOfBirth>
    <Email>charlie@example.com</Email>
    <Phone>+962700000003</Phone>
    <Address>Zarqa</Address>
    <PhotoBase64></PhotoBase64>
    <PhotoSizeBytes>0</PhotoSizeBytes>
  </BusinessCard>
  <BusinessCard>
    <Name>Dina Noor</Name>
    <Gender>Female</Gender>
    <DateOfBirth>1995-12-30</DateOfBirth>
    <Email>dina@example.com</Email>
    <Phone>+962700000004</Phone>
    <Address>Aqaba</Address>
    <PhotoBase64></PhotoBase64>
    <PhotoSizeBytes>0</PhotoSizeBytes>
  </BusinessCard>
</BusinessCards>
""";
        var content = Encoding.UTF8.GetBytes(xml).AsMultipartFile("file", "sample.xml", "application/xml");
        var resp = await client.PostAsync("/api/BusinessCards/import/xml", content);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("Imported 2");
    }
}
