using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class ExportCsvXmlTests : IClassFixture<TestingWebAppFactory>
{
    private readonly TestingWebAppFactory _factory;
    public ExportCsvXmlTests(TestingWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task ExportCsv_Should_Return_File_With_Header()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            name = "Seed One",
            gender = "Male",
            dateOfBirth = "1990-01-01",
            email = "seed1@example.com",
            phone = "+962700000000",
            address = "Amman",
            photoBase64 = (string?)null,
            photoSizeBytes = (int?)null
        };
        await client.PostAsync("/api/BusinessCards", JsonSerializer.Serialize(payload).AsJson());

        var resp = await client.GetAsync("/api/BusinessCards/export/csv");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var csv = await resp.Content.ReadAsStringAsync();
        csv.Split('\n')[0].Trim().Should().Contain("Id,Name,Gender,DateOfBirth,Email,Phone,Address,PhotoBase64,PhotoSizeBytes");
        csv.Should().Contain("Seed One");
    }

    [Fact]
    public async Task ExportXml_Should_Return_File_With_Root()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            name = "Seed Two",
            gender = "Female",
            dateOfBirth = "1993-02-11",
            email = "seed2@example.com",
            phone = "+962700000001",
            address = "Irbid",
            photoBase64 = (string?)null,
            photoSizeBytes = (int?)null
        };
        await client.PostAsync("/api/BusinessCards", JsonSerializer.Serialize(payload).AsJson());

        var resp = await client.GetAsync("/api/BusinessCards/export/xml");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");
        var xml = await resp.Content.ReadAsStringAsync();
        xml.Should().Contain("<BusinessCards>").And.Contain("<BusinessCard>").And.Contain("Seed Two");
    }
}
