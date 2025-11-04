using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class CreateBusinessCardTests : IClassFixture<TestingWebAppFactory>
{
    private readonly TestingWebAppFactory _factory;
    public CreateBusinessCardTests(TestingWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_ShouldReject_Photo_Over_1MB()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            name = "Too Big",
            gender = "Male",
            dateOfBirth = "1990-01-01",
            email = "x@test.com",
            phone = "000",
            address = "Nowhere",
            photoBase64 = "A",
            photoSizeBytes = 1_048_577
        };
        var resp = await client.PostAsync("/api/BusinessCards", JsonSerializer.Serialize(payload).AsJson());
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("1MB");
    }

    [Fact]
    public async Task Create_Should_Succeed_Minimal()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            name = "Alice",
            gender = "Female",
            dateOfBirth = "1993-02-11",
            email = "alice@example.com",
            phone = "+962700000001",
            address = "Amman",
            photoBase64 = (string?)null,
            photoSizeBytes = (int?)null
        };
        var resp = await client.PostAsync("/api/BusinessCards", JsonSerializer.Serialize(payload).AsJson());
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("Success").And.Contain("Alice");
    }
}
