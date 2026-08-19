namespace NGKutahyaSeramik.UnitTests.Factories;

/// <summary>
/// Servis katmanının beklediği (OriginalFileName, ContentType, Length, Stream) dörtlüsünü üretir —
/// gerçek magic-byte doğrulamasını geçecek minimal, geçerli görsel imzaları içerir (BlogService/
/// PageContentBlockService/BannerService'in ValidateImageAsync'iyle birebir uyumlu).
/// </summary>
public static class ImageUploadFactory
{
    public static (string FileName, string ContentType, long Length, MemoryStream Content) ValidJpeg(string fileName = "test.jpg")
    {
        byte[] bytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
        var stream = new MemoryStream(bytes);
        return (fileName, "image/jpeg", stream.Length, stream);
    }

    public static (string FileName, string ContentType, long Length, MemoryStream Content) ValidPng(string fileName = "test.png")
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
        var stream = new MemoryStream(bytes);
        return (fileName, "image/png", stream.Length, stream);
    }

    public static (string FileName, string ContentType, long Length, MemoryStream Content) ValidWebp(string fileName = "test.webp")
    {
        byte[] bytes = "RIFF____WEBP"u8.ToArray();
        var stream = new MemoryStream(bytes);
        return (fileName, "image/webp", stream.Length, stream);
    }

    /// <summary>Uzantı .jpg ama içerik PNG imzası taşır — magic-byte uyuşmazlığı reddi testleri için.</summary>
    public static (string FileName, string ContentType, long Length, MemoryStream Content) MismatchedSignature(string fileName = "fake.jpg")
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        var stream = new MemoryStream(bytes);
        return (fileName, "image/jpeg", stream.Length, stream);
    }

    public static (string FileName, string ContentType, long Length, MemoryStream Content) DisallowedExtension(string fileName = "test.svg")
    {
        byte[] bytes = "<svg></svg>"u8.ToArray();
        var stream = new MemoryStream(bytes);
        return (fileName, "image/svg+xml", stream.Length, stream);
    }

    public static (string FileName, string ContentType, long Length, MemoryStream Content) Empty(string fileName = "empty.jpg")
    {
        var stream = new MemoryStream();
        return (fileName, "image/jpeg", 0, stream);
    }
}
