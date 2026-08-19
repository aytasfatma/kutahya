using Application.Storage;

namespace NGKutahyaSeramik.UnitTests.Mocks;

/// <summary>
/// Gerçek disk erişimi olmayan, davranışı gözlemlenebilir sahte dosya deposu. Testlerde
/// Save/Delete çağrılarının doğru yapıldığını (ör. "eski görsel silindi mi") doğrulamak için
/// kullanılır — Moq ile salt çağrı-doğrulaması yerine, gerçekçi bir "kayıtlı dosya" durumu tutar.
/// </summary>
public class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _existingFiles = [];

    public List<(string Folder, string FileName)> SaveCalls { get; } = [];
    public List<string> DeleteCalls { get; } = [];

    public bool ThrowOnSave { get; set; }

    public Task<string> SaveAsync(string relativeFolder, Stream content, string fileName)
    {
        if (ThrowOnSave)
        {
            throw new IOException("Test: disk yazma hatası simülasyonu.");
        }

        SaveCalls.Add((relativeFolder, fileName));
        var path = $"/uploads/{relativeFolder.Trim('/')}/{fileName}";

        using var memoryStream = new MemoryStream();
        content.Position = 0;
        content.CopyTo(memoryStream);
        _existingFiles[path] = memoryStream.ToArray();

        return Task.FromResult(path);
    }

    public void Delete(string relativeFilePath)
    {
        DeleteCalls.Add(relativeFilePath);
        _existingFiles.Remove(relativeFilePath);
    }

    public bool Exists(string relativeFilePath) => _existingFiles.ContainsKey(relativeFilePath);

    public Task<Stream> OpenReadAsync(string relativeFilePath)
    {
        if (!_existingFiles.TryGetValue(relativeFilePath, out var bytes))
        {
            throw new FileNotFoundException("Test: dosya bulunamadı.", relativeFilePath);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }
}
