using FluentAssertions;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Anonim kullanıcının hiçbir yönetim panel endpoint'ine erişememesini doğrular. Test host'ta
/// gerçek cookie/Login yönlendirmesi yerine TestAuthHandler kullanıldığı için (bkz.
/// CustomWebApplicationFactory), [Authorize] başarısızlığı 401 (challenge) döner — üretimde bu
/// gerçek cookie middleware'iyle 302 Login yönlendirmesine karşılık gelir. Görev talimatı bu ikisini
/// de kabul edilebilir sonuç olarak tanımlıyor.
/// </summary>
public class AnonymousAccessTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnonymousAccessTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous();
    }

    [Theory]
    [InlineData("/Blog")]
    [InlineData("/Blog/Create")]
    [InlineData("/Page")]
    [InlineData("/Page/Create")]
    [InlineData("/Page/Details/1")]
    [InlineData("/Product")]
    [InlineData("/Product/Create")]
    [InlineData("/PageContentBlock/Create?pageId=1")]
    [InlineData("/News")]
    [InlineData("/News/Create")]
    [InlineData("/News/Edit/1")]
    [InlineData("/NewsCategory")]
    [InlineData("/NewsCategory/Create")]
    [InlineData("/Dealer")]
    [InlineData("/Dealer/Create")]
    [InlineData("/Dealer/Edit/1")]
    [InlineData("/FormSubmission")]
    [InlineData("/FormSubmission/Details/1")]
    [InlineData("/User")]
    [InlineData("/User/Create")]
    [InlineData("/User/Edit/1")]
    [InlineData("/User/ResetPassword/1")]
    public async Task Anonymous_GET_IsDenied(string url)
    {
        var response = await _client.GetAsync(url);

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    [Theory]
    [InlineData("/Blog/Delete/1")]
    [InlineData("/Page/Delete/1")]
    [InlineData("/Product/Delete/1")]
    [InlineData("/News/Delete/1")]
    [InlineData("/NewsCategory/Delete/1")]
    [InlineData("/Dealer/Delete/1")]
    [InlineData("/FormSubmission/Delete/1")]
    [InlineData("/FormSubmission/MarkAsRead/1")]
    [InlineData("/User/Delete/1")]
    [InlineData("/User/Activate/1")]
    [InlineData("/User/Deactivate/1")]
    [InlineData("/User/ResetPassword/1")]
    public async Task Anonymous_POST_IsDenied(string url)
    {
        var response = await _client.PostAsync(url, new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }
}
