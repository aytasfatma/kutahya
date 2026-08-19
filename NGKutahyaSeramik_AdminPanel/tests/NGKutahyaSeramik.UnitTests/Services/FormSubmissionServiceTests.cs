using Application.Forms;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Madde 29 — Formlar. FormSubmissionService Translation/IFileStorageService kullanmıyor (Dealer'dan
/// sonra ikinci örnek). GetPagedAsync gerçek SQLite üzerinde SQL seviyesinde filtreleme+sayfalama
/// test ediliyor — `UseInMemoryDatabase` kullanılmadığı için `.Skip`/`.Take`/`.Where` gerçek SQL'e
/// çevrilip çalıştırılıyor.
/// </summary>
public class FormSubmissionServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly FormSubmissionService _sut;

    public FormSubmissionServiceTests()
    {
        _sut = new FormSubmissionService(new FormSubmissionRepository(_ctx.DbContext), _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private static CreateFormSubmissionRequest ValidContactRequest() => new()
    {
        FormType = FormType.Contact,
        FullName = "Test Kullanıcı",
        Email = "test@example.com",
        Phone = "05551234567",
        Company = "Test Firma",
        Message = "Test mesajı",
        ConsentAccepted = true,
        Subject = "Genel Bilgi"
    };

    private static CreateFormSubmissionRequest ValidRequestInformationRequest() => new()
    {
        FormType = FormType.RequestInformation,
        FullName = "Test Kullanıcı",
        Email = "test@example.com",
        Phone = "05551234567",
        Message = "Bilgi almak istiyorum",
        ConsentAccepted = true,
        ProductCode = "55018167RP",
        ProductName = "AMAZONIT"
    };

    private static CreateFormSubmissionRequest ValidSampleRequestRequest() => new()
    {
        FormType = FormType.SampleRequest,
        FullName = "Test Mimar",
        Email = "mimar@example.com",
        Phone = "05551234567",
        Company = "Mimar Ofisi",
        Message = "Numune talep ediyorum",
        ConsentAccepted = true,
        Address = "Test Adres, İstanbul",
        RequestedProduct = "Amazonit",
        Quantity = 2
    };

    [Fact]
    public async Task CreateSubmissionAsync_WithValidContact_Succeeds()
    {
        var result = await _sut.CreateSubmissionAsync(ValidContactRequest());

        result.Succeeded.Should().BeTrue();
        var page = await _sut.GetPagedAsync(new FormSubmissionQuery());
        page.Items.Should().ContainSingle(s => s.FormType == FormType.Contact && s.Subject == "Genel Bilgi");
    }

    [Fact]
    public async Task CreateSubmissionAsync_WithValidRequestInformation_Succeeds()
    {
        var result = await _sut.CreateSubmissionAsync(ValidRequestInformationRequest());

        result.Succeeded.Should().BeTrue();
        var page = await _sut.GetPagedAsync(new FormSubmissionQuery());
        page.Items.Should().ContainSingle(s => s.ProductCode == "55018167RP" && s.ProductName == "AMAZONIT");
    }

    [Fact]
    public async Task CreateSubmissionAsync_WithValidSampleRequest_Succeeds()
    {
        var result = await _sut.CreateSubmissionAsync(ValidSampleRequestRequest());

        result.Succeeded.Should().BeTrue();
        var page = await _sut.GetPagedAsync(new FormSubmissionQuery());
        page.Items.Should().ContainSingle(s => s.Address == "Test Adres, İstanbul" && s.Quantity == 2);
    }

    [Fact]
    public async Task CreateSubmissionAsync_WithoutFullName_IsRejected()
    {
        var request = ValidContactRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = "  ",
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = request.ConsentAccepted,
            Subject = request.Subject
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Ad soyad");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at.com")]
    [InlineData("")]
    public async Task CreateSubmissionAsync_WithInvalidEmail_IsRejected(string invalidEmail)
    {
        var request = ValidContactRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = request.FullName,
            Email = invalidEmail,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = request.ConsentAccepted,
            Subject = request.Subject
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("e-posta");
    }

    [Fact]
    public async Task CreateSubmissionAsync_WithoutConsent_IsRejected()
    {
        var request = ValidContactRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = false,
            Subject = request.Subject
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("KVKK");
    }

    [Fact]
    public async Task CreateSubmissionAsync_ContactWithoutSubject_IsRejected()
    {
        var request = ValidContactRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = request.ConsentAccepted,
            Subject = null
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Konu");
    }

    [Fact]
    public async Task CreateSubmissionAsync_RequestInformationWithoutProductCode_IsRejected()
    {
        var request = ValidRequestInformationRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = request.ConsentAccepted,
            ProductCode = null,
            ProductName = request.ProductName
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Ürün");
    }

    [Fact]
    public async Task CreateSubmissionAsync_SampleRequestWithZeroQuantity_IsRejected()
    {
        var request = ValidSampleRequestRequest();
        var invalid = new CreateFormSubmissionRequest
        {
            FormType = request.FormType,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            ConsentAccepted = request.ConsentAccepted,
            Address = request.Address,
            RequestedProduct = request.RequestedProduct,
            Quantity = 0
        };

        var result = await _sut.CreateSubmissionAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Adet");
    }

    [Fact]
    public async Task CreateSubmissionAsync_NewSubmission_IsUnreadByDefault()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());

        var page = await _sut.GetPagedAsync(new FormSubmissionQuery());
        var submission = page.Items.Single();

        submission.IsRead.Should().BeFalse();
        submission.ReadAt.Should().BeNull();
        submission.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsReadAsync_SetsIsReadAndReadAt()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        var id = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items.Single().Id;

        var result = await _sut.MarkAsReadAsync(id);

        result.Succeeded.Should().BeTrue();
        var submission = await _sut.GetByIdAsync(id);
        submission!.IsRead.Should().BeTrue();
        submission.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsUnreadAsync_ClearsIsReadAndReadAt()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        var id = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items.Single().Id;
        await _sut.MarkAsReadAsync(id);

        await _sut.MarkAsUnreadAsync(id);

        var submission = await _sut.GetByIdAsync(id);
        submission!.IsRead.Should().BeFalse();
        submission.ReadAt.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ThenUnprocessed_TogglesProcessedAt()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        var id = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items.Single().Id;

        await _sut.MarkAsProcessedAsync(id);
        (await _sut.GetByIdAsync(id))!.IsProcessed.Should().BeTrue();

        await _sut.MarkAsUnprocessedAsync(id);
        (await _sut.GetByIdAsync(id))!.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAdminNoteAsync_SetsAndClearsNote()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        var id = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items.Single().Id;

        await _sut.UpdateAdminNoteAsync(id, "Müşteriyle görüşüldü.");
        (await _sut.GetByIdAsync(id))!.AdminNote.Should().Be("Müşteriyle görüşüldü.");

        await _sut.UpdateAdminNoteAsync(id, "  ");
        (await _sut.GetByIdAsync(id))!.AdminNote.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByFormType()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        await _sut.CreateSubmissionAsync(ValidSampleRequestRequest());

        var page = await _sut.GetPagedAsync(new FormSubmissionQuery { FormType = FormType.SampleRequest });

        page.Items.Should().ContainSingle();
        page.Items.Single().FormType.Should().Be(FormType.SampleRequest);
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByIsRead()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        await _sut.CreateSubmissionAsync(ValidSampleRequestRequest());
        var firstId = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items
            .Single(s => s.FormType == FormType.Contact).Id;
        await _sut.MarkAsReadAsync(firstId);

        var unread = await _sut.GetPagedAsync(new FormSubmissionQuery { IsRead = false });
        var read = await _sut.GetPagedAsync(new FormSubmissionQuery { IsRead = true });

        unread.TotalCount.Should().Be(1);
        read.TotalCount.Should().Be(1);
        read.Items.Single().Id.Should().Be(firstId);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByDateRange()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());

        var future = await _sut.GetPagedAsync(new FormSubmissionQuery { CreatedFrom = DateTime.UtcNow.AddDays(1) });
        var past = await _sut.GetPagedAsync(new FormSubmissionQuery { CreatedFrom = DateTime.UtcNow.AddDays(-1) });

        future.TotalCount.Should().Be(0);
        past.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_SearchesByNameEmailPhone()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        await _sut.CreateSubmissionAsync(ValidSampleRequestRequest());

        var byName = await _sut.GetPagedAsync(new FormSubmissionQuery { SearchTerm = "Mimar" });

        byName.TotalCount.Should().Be(1);
        byName.Items.Single().FormType.Should().Be(FormType.SampleRequest);
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesAndOrdersByCreatedAtDescending()
    {
        for (var i = 0; i < 5; i++)
        {
            await _sut.CreateSubmissionAsync(ValidContactRequest());
        }

        var firstPage = await _sut.GetPagedAsync(new FormSubmissionQuery { PageNumber = 1, PageSize = 2 });
        var secondPage = await _sut.GetPagedAsync(new FormSubmissionQuery { PageNumber = 2, PageSize = 2 });

        firstPage.Items.Should().HaveCount(2);
        secondPage.Items.Should().HaveCount(2);
        firstPage.TotalCount.Should().Be(5);
        firstPage.TotalPages.Should().Be(3);
        firstPage.Items.Select(i => i.Id).Should().NotIntersectWith(secondPage.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task DeleteAsync_RemovesSubmission()
    {
        await _sut.CreateSubmissionAsync(ValidContactRequest());
        var id = (await _sut.GetPagedAsync(new FormSubmissionQuery())).Items.Single().Id;

        var result = await _sut.DeleteAsync(id);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByIdAsync(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentSubmission_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync(999);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistentSubmission_ReturnsFailure()
    {
        var result = await _sut.MarkAsReadAsync(999);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }
}
