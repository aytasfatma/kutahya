using FluentAssertions;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;

namespace NGKutahyaSeramik.IntegrationTests.Security;

/// <summary>
/// Gerçek AntiForgery + PRG (Post-Redirect-Get) davranışını Page modülü üzerinden uçtan uca
/// doğrular. AntiForgery hiçbir noktada devre dışı bırakılmaz.
/// </summary>
public class AntiForgeryAndPrgTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AntiForgeryAndPrgTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_WithValidTokenAndValidData_RedirectsToIndex_PRG()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Page/Create",
            postUrl: "/Page/Create",
            formValues: new Dictionary<string, string>
            {
                ["Translations[0].LanguageId"] = "1",
                ["Translations[0].LanguageCode"] = "TR",
                ["Translations[0].LanguageName"] = "Türkçe",
                ["Translations[0].Title"] = "AntiForgery Test Sayfası"
            });

        ((int)response.StatusCode).Should().Be(302, "başarılı POST sonrası PRG deseniyle Index'e yönlendirilmeli");
        response.Headers.Location!.ToString().Should().Contain("/Page");
    }

    [Fact]
    public async Task Create_WithoutAntiForgeryToken_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        // Token/cookie hiç alınmadan doğrudan POST edilir.
        var response = await client.PostAsync("/Page/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Translations[0].LanguageId"] = "1",
            ["Translations[0].Title"] = "Token Yok"
        }));

        ((int)response.StatusCode).Should().Be(400, "AntiForgery doğrulaması token/cookie olmadan başarısız olmalı");
    }

    [Fact]
    public async Task Create_WithInvalidModelState_DoesNotRedirect_ReturnsFormWithErrors()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        // TR başlığı boş bırakılıyor — PageService.ValidateAsync bunu reddeder, PRG uygulanmaz,
        // form 200 ile (validasyon hatalarıyla) tekrar render edilir.
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Page/Create",
            postUrl: "/Page/Create",
            formValues: new Dictionary<string, string>
            {
                ["Translations[0].LanguageId"] = "1",
                ["Translations[0].LanguageCode"] = "TR",
                ["Translations[0].LanguageName"] = "Türkçe"
            });

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task News_Create_WithValidTokenAndValidData_RedirectsToIndex_PRG()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/News/Create",
            postUrl: "/News/Create",
            formValues: new Dictionary<string, string>
            {
                ["Status"] = "Draft",
                ["Translations[0].LanguageId"] = "1",
                ["Translations[0].LanguageCode"] = "TR",
                ["Translations[0].LanguageName"] = "Türkçe",
                ["Translations[0].Title"] = "AntiForgery Test Haberi"
            });

        ((int)response.StatusCode).Should().Be(302, "başarılı POST sonrası PRG deseniyle Index'e yönlendirilmeli");
        response.Headers.Location!.ToString().Should().Contain("/News");
    }

    [Fact]
    public async Task News_Create_WithoutAntiForgeryToken_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        var response = await client.PostAsync("/News/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Status"] = "Draft",
            ["Translations[0].LanguageId"] = "1",
            ["Translations[0].Title"] = "Token Yok"
        }));

        ((int)response.StatusCode).Should().Be(400, "AntiForgery doğrulaması token/cookie olmadan başarısız olmalı");
    }

    [Fact]
    public async Task News_Create_WithInvalidModelState_DoesNotRedirect_ReturnsFormWithErrors()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();

        // TR başlığı boş bırakılıyor — NewsService.ValidateMetadataAsync bunu reddeder, PRG uygulanmaz.
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/News/Create",
            postUrl: "/News/Create",
            formValues: new Dictionary<string, string>
            {
                ["Status"] = "Draft",
                ["Translations[0].LanguageId"] = "1",
                ["Translations[0].LanguageCode"] = "TR",
                ["Translations[0].LanguageName"] = "Türkçe"
            });

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task Dealer_Create_WithValidTokenAndValidData_RedirectsToIndex_PRG()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Dealer/Create",
            postUrl: "/Dealer/Create",
            formValues: new Dictionary<string, string>
            {
                ["Name"] = "AntiForgery Test Bayi",
                ["Category"] = "SalesPoint",
                ["City"] = "İstanbul"
            });

        ((int)response.StatusCode).Should().Be(302, "başarılı POST sonrası PRG deseniyle Index'e yönlendirilmeli");
        response.Headers.Location!.ToString().Should().Contain("/Dealer");
    }

    [Fact]
    public async Task Dealer_Create_WithoutAntiForgeryToken_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.PostAsync("/Dealer/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Token Yok",
            ["City"] = "İstanbul"
        }));

        ((int)response.StatusCode).Should().Be(400, "AntiForgery doğrulaması token/cookie olmadan başarısız olmalı");
    }

    [Fact]
    public async Task User_Create_WithValidTokenAndValidData_RedirectsToIndex_PRG()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var email = $"antiforgery-{Guid.NewGuid():N}@test.local";

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/User/Create",
            postUrl: "/User/Create",
            formValues: new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = "Test-P@ssw0rd-123",
                ["ConfirmPassword"] = "Test-P@ssw0rd-123",
                ["IsActive"] = "true",
                ["Role"] = "İçerik Editörü"
            });

        ((int)response.StatusCode).Should().Be(302, "başarılı POST sonrası PRG deseniyle Index'e yönlendirilmeli");
        response.Headers.Location!.ToString().Should().Contain("/User");
    }

    [Fact]
    public async Task User_Create_WithoutAntiForgeryToken_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.PostAsync("/User/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "token-yok@test.local",
            ["Password"] = "Test-P@ssw0rd-123"
        }));

        ((int)response.StatusCode).Should().Be(400, "AntiForgery doğrulaması token/cookie olmadan başarısız olmalı");
    }

    [Fact]
    public async Task User_Create_WithInvalidModelState_DoesNotRedirect_ReturnsFormWithErrors()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        // Parola boş bırakılıyor — UserController.Create bunu reddeder, PRG uygulanmaz.
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/User/Create",
            postUrl: "/User/Create",
            formValues: new Dictionary<string, string>
            {
                ["Email"] = "eksik-parola@test.local",
                ["Role"] = "İçerik Editörü"
            });

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("/User/Delete/does-not-exist")]
    [InlineData("/User/Activate/does-not-exist")]
    [InlineData("/User/Deactivate/does-not-exist")]
    [InlineData("/User/ResetPassword/does-not-exist")]
    public async Task User_StateChangingActions_WithoutAntiForgeryToken_AreRejected(string url)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.PostAsync(url, new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().Be(400);
    }

    [Theory]
    [InlineData("/User/Activate/does-not-exist")]
    [InlineData("/User/Deactivate/does-not-exist")]
    [InlineData("/User/Delete/does-not-exist")]
    public async Task User_StateChangingActions_WithValidToken_ReachAction_PRG(string url)
    {
        // Kullanıcı gerçekten yok — servis "bulunamadı" ile başarısız olur ama (geçerli AntiForgery
        // token'ıyla) bu, AntiForgery katmanını geçtiği (302 PRG) anlamına gelir. Product_Delete
        // testindeki çapraz-sayfa-token deseni (token, sayfaya değil oturuma bağlı).
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/User/Create", postUrl: url, formValues: []);

        ((int)response.StatusCode).Should().Be(302);
    }

    [Fact]
    public async Task Dealer_Create_WithInvalidModelState_DoesNotRedirect_ReturnsFormWithErrors()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        // Şehir boş bırakılıyor — DealerService.Validate bunu reddeder, PRG uygulanmaz.
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Dealer/Create",
            postUrl: "/Dealer/Create",
            formValues: new Dictionary<string, string>
            {
                ["Name"] = "Şehirsiz Bayi"
            });

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task FormSubmission_MarkAsRead_WithoutAntiForgeryToken_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.PostAsync("/FormSubmission/MarkAsRead/999", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().Be(400, "AntiForgery doğrulaması token/cookie olmadan başarısız olmalı");
    }

    [Fact]
    public async Task FormSubmission_MarkAsRead_WithValidToken_ReachesAction_PRG()
    {
        // Var olmayan bir kayıt için MarkAsRead çağrılıyor — servis "bulunamadı" ile başarısız olur
        // ama (geçerli AntiForgery token'ıyla) bu, AntiForgery katmanını geçtiği (302 PRG) anlamına
        // gelir. Token, farklı bir sayfadan (Product/Create) alınıyor — Product_Delete testindeki
        // aynı çapraz-sayfa-token deseni (token, sayfaya değil oturuma bağlı).
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/Product/Create", postUrl: "/FormSubmission/MarkAsRead/999", formValues: []);

        ((int)response.StatusCode).Should().Be(302);
    }
}
