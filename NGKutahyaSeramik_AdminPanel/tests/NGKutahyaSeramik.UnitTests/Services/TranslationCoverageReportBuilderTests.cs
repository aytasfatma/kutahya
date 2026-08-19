using Application.Translations;
using Domain.Enums;
using FluentAssertions;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>Eksik çeviri takibi — saf/statik dönüştürücü (ProductImportReportBuilderTests ile aynı
/// desen). Girdi elle kurulmuş; DB/dosya gerekmez.</summary>
public class TranslationCoverageReportBuilderTests
{
    private static readonly IReadOnlyList<LanguageInfo> TwoLanguages =
    [
        new LanguageInfo(1, "TR", "Türkçe"),
        new LanguageInfo(2, "EN", "English")
    ];

    private static readonly IReadOnlyList<(EntityType Type, string Label)> TwoTypes =
    [
        (EntityType.Product, "Ürün"),
        (EntityType.Category, "Kategori")
    ];

    private static MissingTranslationDto Item(EntityType type, string label, int entityId, int languageId, string languageCode, string fieldName) => new()
    {
        EntityType = type,
        ModuleLabel = label,
        EntityId = entityId,
        DisplayName = $"Kayıt #{entityId}",
        LanguageId = languageId,
        LanguageCode = languageCode,
        LanguageName = languageCode == "TR" ? "Türkçe" : "English",
        FieldName = fieldName
    };

    [Fact]
    public void Build_NoMissingItems_ReturnsZeroTotal_AndZeroForEveryLanguageAndModule()
    {
        var report = TranslationCoverageReportBuilder.Build([], TwoLanguages, TwoTypes);

        report.TotalMissing.Should().Be(0);
        report.ByLanguage.Should().HaveCount(2);
        report.ByLanguage.Should().OnlyContain(l => l.MissingCount == 0);
        report.ByModule.Should().HaveCount(2);
        report.ByModule.Should().OnlyContain(m => m.MissingCount == 0);
    }

    [Fact]
    public void Build_TotalMissing_EqualsItemCount()
    {
        var items = new List<MissingTranslationDto>
        {
            Item(EntityType.Product, "Ürün", 1, 2, "EN", "Name"),
            Item(EntityType.Product, "Ürün", 1, 2, "EN", "ShortDescription"),
            Item(EntityType.Category, "Kategori", 5, 2, "EN", "Name")
        };

        var report = TranslationCoverageReportBuilder.Build(items, TwoLanguages, TwoTypes);

        report.TotalMissing.Should().Be(3);
    }

    [Fact]
    public void Build_ByLanguage_GroupsCorrectly_AndIncludesLanguagesWithZeroMissing()
    {
        var items = new List<MissingTranslationDto>
        {
            Item(EntityType.Product, "Ürün", 1, 2, "EN", "Name"),
            Item(EntityType.Product, "Ürün", 2, 2, "EN", "Name")
        };

        var report = TranslationCoverageReportBuilder.Build(items, TwoLanguages, TwoTypes);

        report.ByLanguage.Single(l => l.LanguageCode == "EN").MissingCount.Should().Be(2);
        report.ByLanguage.Single(l => l.LanguageCode == "TR").MissingCount.Should().Be(0, "TR için hiç eksik eklenmedi");
    }

    [Fact]
    public void Build_ByModule_GroupsCorrectly_AndIncludesModulesWithZeroMissing()
    {
        var items = new List<MissingTranslationDto>
        {
            Item(EntityType.Product, "Ürün", 1, 1, "TR", "SeoUrl"),
            Item(EntityType.Product, "Ürün", 1, 1, "TR", "MetaTitle"),
            Item(EntityType.Product, "Ürün", 1, 1, "TR", "MetaDescription")
        };

        var report = TranslationCoverageReportBuilder.Build(items, TwoLanguages, TwoTypes);

        report.ByModule.Single(m => m.EntityType == EntityType.Product).MissingCount.Should().Be(3);
        report.ByModule.Single(m => m.EntityType == EntityType.Category).MissingCount.Should().Be(0, "Kategori için hiç eksik eklenmedi");
    }

    [Fact]
    public void Build_Items_IsPassedThroughUnchanged()
    {
        var items = new List<MissingTranslationDto> { Item(EntityType.Product, "Ürün", 7, 2, "EN", "Name") };

        var report = TranslationCoverageReportBuilder.Build(items, TwoLanguages, TwoTypes);

        report.Items.Should().ContainSingle();
        report.Items[0].EntityId.Should().Be(7);
        report.Items[0].FieldName.Should().Be("Name");
    }
}
