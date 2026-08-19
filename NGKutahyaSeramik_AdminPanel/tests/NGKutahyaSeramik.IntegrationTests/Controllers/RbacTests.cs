using FluentAssertions;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.IntegrationTests.Security;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Madde 30 RBAC matrisinin gerçek middleware üzerinden ([Authorize(Roles=...)]) doğrulanması.
/// Page modülü özellikle test ediliyor çünkü Task 11'in bildiği açık teknik borcu somutlaştırıyor:
/// SEO Editörü sayfaları görüntüleyebilir (ViewRoles) ama hiçbir alanı düzenleyemez (EditRoles'ta değil).
/// </summary>
public class RbacTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RbacTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client() => _factory.CreateClient(new() { AllowAutoRedirect = false });

    [Theory]
    [InlineData("Admin")]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    public async Task Page_Index_AllowedFor_ViewRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/Page");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task Page_Index_DeniedFor_ProductManager()
    {
        var response = await Client().AsProductManager().GetAsync("/Page");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Page_Create_AllowedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().GetAsync("/Page/Create");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task Page_Create_DeniedFor_SeoEditor()
    {
        // Task 11 teknik borcu: SEO Editörü Madde 30'da "Meta Alanları" düzenleyebilmeli, ama
        // alan-seviyeli RBAC altyapısı olmadığı için mevcut konvansiyonla tamamen salt-görüntüleme.
        var response = await Client().AsSeoEditor().GetAsync("/Page/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Page_Delete_DeniedFor_SeoEditor()
    {
        var response = await Client().AsSeoEditor().PostAsync("/Page/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task PageContentBlock_Create_DeniedFor_SeoEditor()
    {
        var response = await Client().AsSeoEditor().GetAsync("/PageContentBlock/Create?pageId=1");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Product_Create_AllowedFor_ProductManager()
    {
        var response = await Client().AsProductManager().GetAsync("/Product/Create");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task Product_Create_DeniedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().GetAsync("/Product/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Product_Delete_DeniedFor_ProductManager_OnlyAdminAllowed()
    {
        var response = await Client().AsProductManager().PostAsync("/Product/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Product_Delete_AuthorizedFor_Admin_ReachesAction()
    {
        // Ürün gerçekten yok, servis "bulunamadı" ile başarısız olur ama (geçerli AntiForgery token'ıyla)
        // bu RBAC katmanını geçtiği (302 PRG) anlamına gelir — 401/403 olmaması yetkilendirmenin
        // başarılı olduğunu kanıtlar. /Product/Create GET, Delete'in aynı sayfadaki formundan değil
        // ama antiforgery cookie'si aynı client için Index üzerinden de alınabilir.
        var client = Client().AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/Product/Create", postUrl: "/Product/Delete/1", formValues: []);

        ((int)response.StatusCode).Should().Be(302);
    }

    [Fact]
    public async Task Blog_Create_DeniedFor_SeoEditor()
    {
        var response = await Client().AsSeoEditor().GetAsync("/Blog/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Blog_Index_AllowedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().GetAsync("/Blog");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    public async Task News_Index_AllowedFor_ViewRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/News");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task News_Index_DeniedFor_ProductManager()
    {
        var response = await Client().AsProductManager().GetAsync("/News");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task News_Create_AllowedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().GetAsync("/News/Create");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task News_Create_DeniedFor_SeoEditor()
    {
        // Madde 30 SEO Editörü'ne "Meta Alanları" düzenleme yetkisi veriyor, ama projede hiçbir modülde
        // alan-seviyeli RBAC yok (Task 9'dan beri bilinçli, Blog/Page ile tutarlı) — tamamen salt-görüntüleme.
        var response = await Client().AsSeoEditor().GetAsync("/News/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task News_Delete_DeniedFor_SeoEditor()
    {
        var response = await Client().AsSeoEditor().PostAsync("/News/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task NewsCategory_Create_DeniedFor_SeoEditor()
    {
        var response = await Client().AsSeoEditor().GetAsync("/NewsCategory/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Dealer_Index_AllowedFor_Admin()
    {
        var response = await Client().AsAdmin().GetAsync("/Dealer");

        ((int)response.StatusCode).Should().Be(200);
    }

    // Madde 30 Bayi/Showroom satırı: Admin=Tam, diğer 3 rolün (İçerik Editörü/SEO Editörü/Ürün
    // Yöneticisi) HİÇBİR erişimi yok — projedeki ilk salt-Admin modül (diğer modüllerde en azından
    // salt-görüntüleme kalıyordu).
    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Dealer_Index_DeniedFor_AllNonAdminRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/Dealer");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Dealer_Create_AllowedFor_Admin()
    {
        var response = await Client().AsAdmin().GetAsync("/Dealer/Create");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Dealer_Create_DeniedFor_AllNonAdminRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/Dealer/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Dealer_Delete_DeniedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().PostAsync("/Dealer/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // Madde 30 Form Yönetimi satırı: Admin=Tam, İçerik Editörü=Görüntüleme, SEO Editörü=—,
    // Ürün Yöneticisi=—. Blog/News'in ViewRoles/EditRoles ayrımına benzer ama EditRoles yalnızca
    // Admin (İçerik Editörü'nün buradaki "Görüntüleme"si Blog/News'teki SEO Editörü'nün
    // salt-görüntülemesiyle aynı anlamda — hiçbir yazma yetkisi yok).
    [Theory]
    [InlineData("Admin")]
    [InlineData("İçerik Editörü")]
    public async Task FormSubmission_Index_AllowedFor_ViewRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/FormSubmission");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task FormSubmission_Index_DeniedFor_NonViewRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/FormSubmission");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task FormSubmission_MarkAsRead_DeniedFor_ContentEditor()
    {
        // İçerik Editörü yalnızca "Görüntüleme" yetkisine sahip — hiçbir yazma action'ına erişemez.
        var response = await Client().AsContentEditor().PostAsync("/FormSubmission/MarkAsRead/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task FormSubmission_Delete_DeniedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().PostAsync("/FormSubmission/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // Madde 30 Kullanıcı Yönetimi satırı: Admin=Tam, diğer 3 rolün HİÇBİR erişimi yok — Dealer/
    // Bayi-Showroom ile aynı salt-Admin desen.
    [Fact]
    public async Task User_Index_AllowedFor_Admin()
    {
        var response = await Client().AsAdmin().GetAsync("/User");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task User_Index_DeniedFor_AllNonAdminRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/User");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task User_Create_AllowedFor_Admin()
    {
        var response = await Client().AsAdmin().GetAsync("/User/Create");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task User_Create_DeniedFor_AllNonAdminRoles(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/User/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task User_Delete_DeniedFor_ContentEditor()
    {
        var response = await Client().AsContentEditor().PostAsync("/User/Delete/1", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task AuthenticatedWithoutPanelRole_IsDeniedEverywhere()
    {
        var response = await Client().AsAuthenticatedWithoutRole().GetAsync("/Page");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }
}
