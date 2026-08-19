using Application.Translations;
using Domain.Entities;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class LanguageFactory
{
    public static Language CreateTurkish(int displayOrder = 1) => new("TR", "Türkçe", isActive: true, displayOrder);

    public static Language CreateEnglish(int displayOrder = 2) => new("EN", "English", isActive: true, displayOrder);

    /// <summary>ITranslationService.GetActiveLanguagesAsync ile aynı şekli (LanguageInfo) üretir.</summary>
    public static IReadOnlyList<LanguageInfo> CreateDefaultActiveLanguageInfos() =>
    [
        new LanguageInfo(1, "TR", "Türkçe"),
        new LanguageInfo(2, "EN", "English")
    ];
}
