using Application.Seo;
using FluentAssertions;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>Backlog #4 — SEO URL normalizasyonu (duplicate karşılaştırması için). Saf, bağımlılıksız
/// fonksiyon — DB/servis gerekmez.</summary>
public class SeoUrlNormalizerTests
{
    [Theory]
    [InlineData("Amazonit 120x280 Parlak", "amazonit-120x280-parlak")]
    [InlineData("  boşluklu   deger  ", "bosluklu-deger")]
    [InlineData("Çorap-Çeşitleri_Ürünü", "corap-cesitleri-urunu")]
    [InlineData("İstanbul Ürünü İçin", "istanbul-urunu-icin")]
    [InlineData("already-normalized-slug", "already-normalized-slug")]
    [InlineData("---leading-trailing---", "leading-trailing")]
    [InlineData("a!!!b???c", "a-b-c")]
    public void Normalize_ProducesExpectedSlug(string input, string expected)
    {
        SeoUrlNormalizer.Normalize(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_BlankInput_ReturnsEmptyString(string? input)
    {
        SeoUrlNormalizer.Normalize(input).Should().BeEmpty();
    }

    [Fact]
    public void Normalize_IsCaseInsensitive_SameResultForDifferentCasing()
    {
        SeoUrlNormalizer.Normalize("Amazonit").Should().Be(SeoUrlNormalizer.Normalize("AMAZONIT"));
        SeoUrlNormalizer.Normalize("Amazonit").Should().Be(SeoUrlNormalizer.Normalize("amazonit"));
    }
}
