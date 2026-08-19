using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using NGKutahyaSeramik.IntegrationTests.Fixtures;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

public sealed class PublicCatalogApiTests
{
    [Theory]
    [InlineData("/api/public/categories")]
    [InlineData("/api/public/collections")]
    [InlineData("/api/public/documents")]
    [InlineData("/api/public/projects")]
    [InlineData("/api/public/banners")]
    [InlineData("/api/public/pages")]
    [InlineData("/api/public/dealers")]
    [InlineData("/api/public/languages")]
    public async Task ListEndpoints_AreAnonymous_AndReturnJson(string url)
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Products_IsAnonymous_AndReturnsPagedContract()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/products?page=1&pageSize=12&lang=tr");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
        json.GetProperty("page").GetInt32().Should().Be(1);
        json.GetProperty("pageSize").GetInt32().Should().Be(12);
        json.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        json.TryGetProperty("totalPages", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/public/blogs?page=1&pageSize=6&lang=tr")]
    [InlineData("/api/public/news?page=1&pageSize=6&lang=tr")]
    public async Task ContentLists_AreAnonymous_AndReturnPagedContract(string url)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient().GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
        json.GetProperty("page").GetInt32().Should().Be(1);
        json.GetProperty("pageSize").GetInt32().Should().Be(6);
    }

    [Theory]
    [InlineData("/api/public/products/does-not-exist")]
    [InlineData("/api/public/projects/does-not-exist")]
    [InlineData("/api/public/blogs/does-not-exist")]
    [InlineData("/api/public/news/does-not-exist")]
    [InlineData("/api/public/pages/does-not-exist")]
    public async Task DetailEndpoints_UnknownSeoUrl_ReturnNotFound(string url)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient().GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FormEndpoint_ValidContact_CreatesSubmission()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient().PostAsJsonAsync("/api/public/forms", new
        {
            formType = "Contact",
            fullName = "Public API Test",
            email = "public@example.com",
            phone = "+90 555 000 00 00",
            subject = "İletişim",
            message = "REST API form testi",
            consentAccepted = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("succeeded").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task FormEndpoint_InvalidPayload_ReturnsValidationProblem()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient().PostAsJsonAsync("/api/public/forms", new
        {
            formType = "Contact",
            fullName = "",
            email = "invalid",
            phone = "",
            message = "",
            consentAccepted = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Object);
    }
}
