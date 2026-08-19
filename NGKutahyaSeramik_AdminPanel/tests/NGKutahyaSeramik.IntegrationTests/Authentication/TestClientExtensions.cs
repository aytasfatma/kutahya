using Infrastructure.Identity;

namespace NGKutahyaSeramik.IntegrationTests.Authentication;

/// <summary>Okunabilir, rol-bazlı authenticated HttpClient üretimi (kullanıcı talimatındaki
/// "rol bazlı authenticated client üretimi" ihtiyacı).</summary>
public static class TestClientExtensions
{
    public static HttpClient AsRole(this HttpClient client, string role)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        return client;
    }

    public static HttpClient AsAdmin(this HttpClient client) => client.AsRole(ApplicationRoles.Admin);

    public static HttpClient AsContentEditor(this HttpClient client) => client.AsRole(ApplicationRoles.ContentEditor);

    public static HttpClient AsSeoEditor(this HttpClient client) => client.AsRole(ApplicationRoles.SeoEditor);

    public static HttpClient AsProductManager(this HttpClient client) => client.AsRole(ApplicationRoles.ProductManager);

    /// <summary>Authenticated ama hiçbir panel rolüne sahip olmayan kullanıcı — "yetkisiz authenticated
    /// kullanıcı" senaryosu için (header'a boş-olmayan ama tanınmayan bir rol adı konur).</summary>
    public static HttpClient AsAuthenticatedWithoutRole(this HttpClient client) => client.AsRole("NoPanelAccessRole");

    /// <summary>Anonim (hiç Authorization/rol header'ı yok) — varsayılan HttpClient hali zaten budur,
    /// bu metot yalnızca niyeti testte açıkça okunur kılmak için var.</summary>
    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        return client;
    }
}
