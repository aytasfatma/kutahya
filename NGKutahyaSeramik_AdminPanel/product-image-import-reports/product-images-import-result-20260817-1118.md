# Ürün görsel import sonuç raporu

- Tarih: 2026-08-17
- Durum: Başarılı
- İşlenen ürün: 1.124
- Yeni kaynak görseli bulunan ürün: 1.108
- Fallback `deneme.webp` kullanan ürün: 16
- Eklenen `ProductImage` kaydı: 8.002
- Kalıcı fiziksel dosya: 7.987
- Bölünen kaynak dosya: 8
- İki parçaya bölünen kaynak: 4 (8 çıktı)
- Üç parçaya bölünen kaynak: 4 (12 çıktı)
- Kaldırılan eski `deneme.webp` kaydı: 0 (başlangıç veritabanında yoktu)
- Atlanan veya belirsiz dosya: 0
- Son import hatası: 0
- Ürün başına en yüksek görsel sayısı: 24
- Altıdan fazla görseli bulunan ürün: 525 (istenen şekilde kırpılmadı)

## Bütünlük doğrulaması

- Görselli ürün: 1.124 / 1.124
- Görselsiz ürün: 0
- Tam olarak bir ana görseli olmayan ürün: 0
- Ana görseli ilk sırada olmayan ürün: 0
- Kesintisiz olmayan `DisplayOrder` dizisi: 0
- Aynı üründe yinelenen dosya yolu: 0
- Sıfır baytlık dosya: 0
- Fallback veritabanı kaydı: 16
- Fallback fiziksel dosyası: 1
- Eski/geçersiz placeholder kaydı: 0

## HTTP doğrulaması

Rastgele yedi ürün ile bir ikili pano, bir üçlü pano ve bir fallback ürününden seçilen toplam on URL kontrol edildi. Tamamı HTTP 200 ve `image/webp` döndürdü.

## Saklama ve güvenlik

- Dosyalar Docker `uploads-data` volume'unda `/app/wwwroot/uploads/products/imported` altında saklanıyor.
- Kaynak görseller salt okunur mount edildi; silinmedi veya üzerlerine yazılmadı.
- Tekli WebP kaynaklar kalite kaybı olmadan byte-for-byte kopyalandı.
- Yalnızca ikili ve üçlü panolar WebP quality 92 ile parçalara kodlandı.
- Veritabanı değişiklikleri tek transaction içinde commit edildi.
- Mevcut yedekler: çalışma alanındaki `NGKutahyaSeramikAdminPanel_current.bak` ve `NGKutahyaSeramikAdminPanel_before_restore_20260816.bak`.

## Çalışma notu

İlk apply denemesinde tüm WebP dosyalarının yeniden kodlanmasının gereksiz derecede yavaş olduğu görüldü. Transaction commit edilmeden süreç kontrollü olarak durduruldu; veritabanında 0 kayıt olduğu doğrulandı ve yalnızca yarım import dizini temizlendi. Tekli WebP'ler doğrudan kopyalanacak şekilde optimize edilen ikinci çalıştırma 116 saniyede başarıyla tamamlandı.
