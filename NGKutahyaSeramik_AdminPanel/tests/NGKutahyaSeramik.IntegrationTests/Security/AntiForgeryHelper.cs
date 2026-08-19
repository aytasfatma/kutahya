using System.Text.RegularExpressions;

namespace NGKutahyaSeramik.IntegrationTests.Security;

/// <summary>
/// Gerçek AntiForgery akışını (token + cookie) test etmek için: önce formu içeren bir GET isteği
/// atılır (yanıttaki antiforgery cookie'si, `WebApplicationFactory.CreateClient()`'in varsayılan
/// `HandleCookies=true` istemcisi tarafından otomatik saklanır ve sonraki isteklerde otomatik
/// gönderilir), dönen HTML'den gizli `__RequestVerificationToken` alanı çıkarılır ve form body'sine
/// eklenir. AntiForgery testte hiçbir zaman devre dışı bırakılmaz — bu sınıf onu atlamaz, gerçek
/// akışı (cookie + form token eşleşmesi) sürdürür. Aynı HttpClient örneği GET ve POST için
/// kullanılmalıdır (cookie devamlılığı için).
/// </summary>
public static partial class AntiForgeryHelper
{
    public static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string formUrl)
    {
        var response = await client.GetAsync(formUrl);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var tokenMatch = TokenRegex().Match(html);
        if (!tokenMatch.Success)
        {
            throw new InvalidOperationException(
                $"'{formUrl}' sayfasında '__RequestVerificationToken' gizli alanı bulunamadı. Antiforgery token parse edilemedi.");
        }

        return tokenMatch.Groups["token"].Value;
    }

    /// <summary>Aynı client ile önce GET (token+cookie alınır), sonra form body'sinde token ile POST edilir.</summary>
    public static async Task<HttpResponseMessage> PostWithAntiForgeryAsync(
        HttpClient client, string formUrl, string postUrl, Dictionary<string, string> formValues)
    {
        var token = await GetAntiForgeryTokenAsync(client, formUrl);
        formValues["__RequestVerificationToken"] = token;

        return await client.PostAsync(postUrl, new FormUrlEncodedContent(formValues));
    }

    [GeneratedRegex(
        "name=\"__RequestVerificationToken\"[^>]*value=\"(?<token>[^\"]+)\"",
        RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();
}
