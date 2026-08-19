namespace Application.Storage;

public interface IFileStorageService
{
    /// <summary>
    /// Verilen içeriği "relativeFolder" altına "fileName" ismiyle kaydeder ve web-relative
    /// (örn. "/uploads/products/55018167RP/face/&lt;guid&gt;.jpg") yolu döner.
    /// </summary>
    Task<string> SaveAsync(string relativeFolder, Stream content, string fileName);

    /// <summary>
    /// SaveAsync'in döndürdüğü web-relative yola ait fiziksel dosyayı siler. Dosya yoksa sessizce geçer.
    /// </summary>
    void Delete(string relativeFilePath);

    bool Exists(string relativeFilePath);

    /// <summary>Backlog #17 (Excel Import) ile eklendi — önizleme/onay iki aşaması arasında
    /// geçici olarak kaydedilen dosyayı geri okumak için. Diğer tüm mevcut kullanımlar (Product/
    /// Document/Banner/ReferenceProject görselleri) bu metodu hiç kullanmaz, yalnızca Save/Delete/
    /// Exists ile çalışmaya devam eder.</summary>
    Task<Stream> OpenReadAsync(string relativeFilePath);
}
