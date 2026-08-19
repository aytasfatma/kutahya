# ARCHITECTURE DECISIONS — NG Kütahya Seramik Yönetim Paneli

> Her karar: Karar / Sebebi / Alternatifleri / Avantajları / Dezavantajları / Dokümandaki dayanağı
> Eski kayıtlar silinmez; geçersiz kılınan kayıtlar "Geçersiz Kılındı" iziyle işaretlenir.

---

## ADR-001 — Proje Kapsamı: Sadece Yönetim Paneli + Backend API

**Karar:** Bu teslimat, Kavramsal Analiz dokümanının tamamını (public web sitesi dahil) değil, sadece **Yönetim Paneli** ve onu besleyen **Backend API/veri modelini** kapsar. Public web sitesi (ziyaretçi arayüzü, sayfa render'ları, public endpoint'ler, form gönderim endpoint'leri) bu teslimatın dışındadır ve **gelecek faz** olarak kaydedilmiştir.

**Sebebi:** Proje klasörünün adı ve kullanıcının proje başlığı dokümanın tam kapsamıyla örtüşmüyordu. Kullanıcıya doğrudan soruldu ve teyit edildi.

**Alternatifleri:** (A) Tam kapsam: Public site + Panel birlikte. (B) Sadece Panel + API (seçilen).

**Avantajları:** Kapsam netleşiyor, gereksiz public kod üretilmiyor (YAGNI), Panel/API bağımsız test edilip teslim edilebilir.

**Dezavantajları:** Admin-yazma ile ileride gelecek public-okuma sınırının şimdiden mimari olarak doğru ayrılması gerekiyor; bu sınır **henüz teknik olarak tasarlanmadı**, sadece kavramsal olarak not edildi. Public endpoint isimleri, public controller'lar veya "ileride kullanılır" diye herhangi bir public kod bu aşamada YAZILMAYACAK.

**Dokümandaki Dayanağı:** Madde 4.2, Madde 17, Madde 35.3. Kapsam ayrımının kendisi dokümanda yazılı değildir; kullanıcı talimatıyla netleştirilmiştir.

**Kapsam ifadesi (referans):**
- Genel doküman kapsamı: Public web sitesi + Yönetim paneli.
- Mevcut geliştirme ve teslimat kapsamı: Yönetim paneli + Backend API.
- Gelecek faz: Public web sitesi.

---

## ADR-002 — Backend API'nin Kesin Teknik Sınırı (Task 0.2 / Karar #1)

**Karar:** Bu fazda geliştirilecek Backend API **yalnızca** Yönetim Paneli'nin CRUD, yönetim, yetkilendirme ve operasyonel ihtiyaçlarını karşılayacaktır.

**Kapsama dahil olanlar:**
- Yönetim paneli kullanıcı ve rol işlemleri (auth/authz)
- Yönetim paneli modüllerinin CRUD işlemleri
- Panele özel veri listeleme, filtreleme, sıralama, durum yönetimi
- Panelin ihtiyaç duyduğu dosya/görsel/doküman işlemleri
- Swagger/OpenAPI ile admin API dokümantasyonu

**Kapsama dahil olmayanlar:**
- Public web sitesi arayüzü, public read-only endpoint'ler, public controller'lar, public response modelleri
- SAP entegrasyon endpoint'leri
- CRM entegrasyon endpoint'leri
- "İleride kullanılabilir" diye yazılan spekülatif kod

**Kısıt (henüz somut tasarım kararı değil, bir ilke):** Katmanlar ve domain modeli, ileride public site veya SAP/CRM entegrasyonu eklendiğinde mevcut admin modüllerinin **yeniden yazılmasını gerektirmeyecek** şekilde sorumlulukları ayrılmış ve genişletilebilir tasarlanacaktır. Bu ilkenin somut karşılığı (katman sınırları, hangi projede ne duracağı) Karar #2 ve sonraki kararlarda netleşecektir.

**Sebebi:** YAGNI; kullanıcının Task 0.1 onayındaki net talimatı ("public endpoint tasarlama/isimlendirme yapma"); ADR-001'in somutlaştırılması.

**Alternatifleri:** (B) Admin CRUD API + spekülatif public-read API birlikte — reddedildi (dokümanda tanımlı olmayan, ihtiyacı netleşmemiş kod riski). (C) Admin API + SAP/CRM entegrasyon endpoint'leri birlikte — reddedildi (Madde 40 bunu açıkça Faz 2'ye koyuyor, ICD kesinleşmedi).

**Avantajları:** Kapsam netliği, YAGNI uyumu, efor/süre tahmini kolaylaşır, ADR-001 ile tam tutarlı.

**Dezavantajları:** Public site veya SAP/CRM eklendiğinde ek bir entegrasyon/API katmanı işi gündeme gelecek — bilinçli olarak kabul edilen, katman ayrımıyla azaltılacak bir risktir.

**Dokümandaki Dayanağı:** Madde 35.3 (API'nin SAP/CRM bağlamında tanımlanması), Madde 39 ("SAP API Endpointleri — Web Sitesi Sağlayacak"), Madde 40 (SAP API Entegrasyonu'nun Faz 2'de olması), Madde 4.2, Madde 17. Kapsam ayrımının kendisi dokümanda yazılı değildir; ADR-001'in devamı olarak kullanıcı kararıyla netleşmiştir.

**Durum:** Onaylandı (kullanıcı, 17.07.2026).

---

## ADR-003 — Uygulama Tipi ve Panelin Sunum Modeli (Task 0.2 / Karar #2)

**Karar:** Yönetim paneli, **server-rendered ASP.NET Core MVC** (Views + Controllers) kullanılarak geliştirilecektir.

**Kesin kapsam:**
- Panel arayüzü MVC Views ve Controllers ile oluşturulacak.
- Responsive HTML5/CSS3/JavaScript ile geliştirilecek (Madde 4.3'teki "Frontend: Responsive HTML5/CSS3/JS" şartı **panel arayüzü için de geçerli** kabul edilmiştir).
- React, Angular, Vue veya başka bir SPA framework'ü eklenmeyecek.
- Razor Pages kullanılmayacak.
- Panel için ayrı bir SPA frontend uygulaması oluşturulmayacak.
- Bu karar, tüm katmanların tek proje içinde olacağı anlamına **gelmez**: Solution, katmanlı mimariye uygun şekilde ayrıştırılabilir. MVC projesi **Presentation** katmanını temsil eder ve çalıştırılabilir web uygulamasıdır; Domain, Application ve Infrastructure sorumlulukları Presentation'dan ayrılacaktır.
- Controller'lar doğrudan veri erişimi yapmayacak ve iş kurallarını içermeyecek (yalnızca Application katmanını çağıracak).
- View'lar yalnızca sunum sorumluluğu taşıyacak.
- Dinamik panel işlemlerinde ihtiyaç halinde sade JavaScript kullanılabilir; gereksiz frontend framework/kütüphane eklenmeyecek.

**Bu ADR'nin KESİNLEŞTİRMEDİĞİ konu:** Authentication yöntemi (ASP.NET Core Identity, Cookie authentication veya JWT) bu kararla **kesinleştirilmemiştir**. MVC seçimi Cookie authentication için doğal destek sunar, ancak bu bir zorunluluk değildir; authentication yöntemi **Karar #4**'te ayrıca değerlendirilecektir.

**Sebebi:** Madde 35.1'in kendi terminolojisi ("Presentation Layer — Views/Controllers"); dokümanda tanımlı olmayan bir SPA teknolojisi eklenmemesi; ADR-002'nin genişletilebilirlik ilkesinin katman ayrımıyla (Controller'ların sadece Application katmanını çağırması) korunması.

**Alternatifleri:** (2) Web API + ayrı SPA frontend — reddedildi (dokümanda dayanağı yok, kapsamı büyütür, JWT/CORS ek karmaşıklık). (3) Razor Pages — reddedildi (doküman terminolojisiyle "Controllers" kavramı örtüşmüyor, çok-modüllü/karmaşık ilişkili ekranlar için MVC'nin controller-action esnekliği daha uygun).

**Avantajları:** Doküman terminolojisiyle tam uyum, ek teknoloji riski yok, Katmanlı Mimari ile doğal birleşme, tek solution/tek build pipeline.

**Dezavantajları:** Zengin/dinamik UI etkileşimleri için elle JavaScript yazımı gerekebilir — bu, "sade JavaScript kullanılabilir, gereksiz framework eklenmeyecek" ilkesiyle sınırlı tutulacak, kabul edilen bir dezavantajdır.

**Dokümandaki Dayanağı:** Madde 35.1 (Views/Controllers), Madde 17.1 (Yönetim paneli ASP.NET Core, responsive arayüz), Madde 4.3 (Responsive HTML5/CSS3/JS — panel için de geçerli sayılması kullanıcı kararıyla netleşmiştir, doküman bu ayrımı kendisi yapmamıştır).

**Durum:** Onaylandı (kullanıcı, 17.07.2026).

---

## ADR-004 — EF Core Yaklaşımı: Code First (Task 0.2 / Karar #3)

**Karar:** Entity Framework Core **Code First** yaklaşımı kullanılacaktır.

**Kesin kapsam:**
- Veritabanı şeması entity ve configuration sınıflarından üretilecek.
- Şema değişiklikleri EF Core Migration dosyalarıyla yönetilecek; migration dosyaları Git üzerinden takip edilecek ve kod review sürecine dahil edilecek.
- Veritabanında elle yapılan, migration karşılığı bulunmayan kontrolsüz şema değişikliklerinden kaçınılacak.
- Her modülde gereksiz migration üretilmeyecek; anlamlı ve tutarlı şema değişiklikleri migration olarak eklenecek.
- Üretim veritabanına migration uygulama yöntemi bu ADR'de **kesinleşmedi** — sonraki bir deployment kararında ayrıca belirlenecek.
- Henüz karara bağlanmayan konular (çoklu dil veri modeli, SEO ilişkileri, Showroom yapısı) kesinleşmeden bunlara ait entity veya migration oluşturulmayacak.
- Dokümanda tanımlanmayan BaseEntity, soft delete, audit alanları, generic repository veya benzeri yapılar sırf yaygın oldukları için eklenmeyecek; ihtiyaç ve doküman dayanağına göre ayrıca değerlendirilecek.

**Sebebi:** Kullanıcı teyidi: bu projede teslim edilmiş/hazır bir SQL Server veritabanı, mevcut tablo şeması, `.bak` dosyası veya legacy database **bulunmuyor** — Database First'ün ön koşulu (var olan bir DB) karşılanmıyor. Doküman Madde 35.2 bu kararı geliştirme ekibine bırakmış ve hiçbir yerde var olan bir DB'den bahsetmiyor; Madde 37 (Excel Import) verinin sıfırdan aktarılacağını gösteriyor (yeşil alan işareti).

**Alternatifi:** Database First — reddedildi (var olan DB/şema/`.bak` yok).

**Avantajları:** Yeşil alana uygun; migration'lar git-dostu ve review edilebilir; veri modelinin hâlâ kısmen açık kararlara bağlı olduğu bu aşamada kademeli ilerlemeye izin verir; entity'ler DB'den bağımsız test edilebilir.

**Dezavantajları:** Hesaplanan/ileri seviye DB özellikleri (örn. Madde 24'teki `FileSize` computed alanı) Fluent API ile ayrıca modellenmeli — kabul edilen küçük bir ek efor.

**Dokümandaki Dayanağı:** Madde 35.2 ("Code-first veya database-first yaklaşımı geliştirme ekibi tarafından belirlenecektir"), Madde 37 (Excel Import ile sıfırdan veri aktarımı).

**Durum:** Onaylandı (kullanıcı, 17.07.2026). Mevcut/hazır veritabanı olmadığı kullanıcı tarafından teyit edilmiştir.

---

## ADR-005 — Authentication ve Authorization: ASP.NET Core Identity + Cookie (Task 0.2 / Karar #4)

**Karar:** Yönetim paneli kullanıcıları **ASP.NET Core Identity** altyapısıyla yönetilecek; authentication yöntemi **Cookie Authentication** olacaktır.

**Kesin kapsam:**
- JWT, Identity+JWT veya hibrit authentication bu fazda **kullanılmayacaktır**. JWT yalnızca ileride SAP/CRM veya ayrı bir client ihtiyacı doğarsa, ilgili fazda ayrıca değerlendirilecektir.
- Oturum, server-rendered ASP.NET Core MVC (ADR-003) ile uyumlu şekilde cookie üzerinden yönetilecektir.
- Cookie ayarları: HttpOnly, Secure, uygun SameSite politikası, makul oturum süresi, Login ve AccessDenied yönlendirmeleri.
- Form gönderimlerinde CSRF koruması; state-changing işlemlerde AntiForgery doğrulaması.
- Parolalar Identity'nin yerleşik password hasher mekanizmasıyla saklanacak; düz metin parola saklanmayacak. Güçlü parola politikası tanımlanacak. Başarısız giriş denemelerinde hesap kilitleme altyapısı kullanılacak.
- Kullanıcı, rol ve yetki yönetimi Identity Role/Claim/Policy mekanizmalarıyla çözülecek. Dört panel rolü: Admin, İçerik Editörü, SEO Editörü, Ürün Yöneticisi.
- Yetkilendirme sadece menü gizleme seviyesinde bırakılmayacak; **Controller ve action seviyesinde server-side authorization** uygulanacak. Gerektiğinde policy tabanlı yetkilendirme kullanılabilir; gereksiz policy oluşturulmayacak.
- Identity'nin altyapısal tabloları kullanılacaktır. Bunların dokümanda tek tek belirtilmemiş olması bir iş gereksinimi eklemek olarak değerlendirilmez — bunlar seçilen framework'ün güvenlik altyapısıdır.
- Özel parola hashleme, özel authentication sistemi veya sıfırdan RBAC altyapısı **yazılmayacaktır**.

**2FA alt kararı (kesinleşen kısım):** Doküman 2FA'yı "opsiyonel" tanımladığı için bu fazda **zorunlu 2FA akışı geliştirilmeyecek**; Identity seçimi sayesinde 2FA desteğine teknik olarak açık kalınacak; şimdiden kullanılmayan 2FA controller/view/servis oluşturulmayacak.

**2FA alt kararı (AÇIK — bu ADR'de kesinleşmedi):** 2FA ekranları, aktivasyon süreci, recovery code yönetimi ve zorunluluk politikası — gelecekte ayrı bir task olarak ele alınacak.

**Şifre sıfırlama (kesinleşen kısım):** Identity'nin şifre sıfırlama yeteneği mimaride desteklenecek (yetenek olarak mimariye kapatılmayacak).

**Şifre sıfırlama (AÇIK — bu ADR'de kesinleşmedi):** E-posta sağlayıcısı ve gerçek mail gönderim altyapısı dokümanda net olmadığı için şimdiden seçilmeyecek. Şifre sıfırlama akışının bu fazda uygulanıp uygulanmayacağı açık karar olarak tutulur.

**Sebebi:** Madde 31'in "güçlü parola politikası + opsiyonel 2FA" ve CSRF zorunluluğu; Madde 30 RBAC gereksinimi; ADR-003'ün server-rendered MVC kararıyla Cookie'nin doğal uyumu; JWT'nin dokümandaki tek dayanağının (Madde 35.3) ADR-002 ile kapsam dışı tutulan SAP/CRM entegrasyonuna ait olması.

**Alternatifleri:** Özel kullanıcı tabloları + Cookie (reddedildi — güvenlik-kritik kodun sıfırdan yazılması riski), JWT (reddedildi — ADR-003 ile uyumsuz, dokümanda dayanağı yok), Identity+JWT ve Hibrit (reddedildi — JWT kısmı spekülatif kod, ADR-002'nin hariç tuttuğu şey).

**Avantajları:** Test edilmiş güvenlik altyapısı (hashleme, kilitleme, 2FA desteği), RBAC'ın Role/Policy ile doğal modellenmesi, Cookie+CSRF'nin MVC ile native uyumu.

**Dezavantajları:** Identity'nin kendi şema alanları (framework getirisi) — bilinçli olarak kabul edilmiştir, iş gereksinimi olarak değerlendirilmemiştir.

**Dokümandaki Dayanağı:** Madde 30 (RBAC), Madde 31 (parola politikası, opsiyonel 2FA, CSRF).

**Durum:** Onaylandı (kullanıcı, 17.07.2026). 2FA detayları ve şifre sıfırlama akışı bilinçli olarak açık bırakılmıştır.

---

## ADR-006 — Dosya, Görsel ve Doküman Saklama: Yerel Dosya Sistemi + Storage Abstraction (Task 0.2 / Karar #5)

**Karar:** Mevcut fazda dosya/görsel/doküman **Yerel Dosya Sistemi** üzerinde saklanacaktır — ancak doğrudan/kontrolsüz bir şekilde değil, bir **storage abstraction** üzerinden.

**Kesin kapsam:**
- Dosyalar uygulama kodundan ve deployment klasöründen ayrı, kalıcı bir fiziksel dizinde saklanacak; doğrudan `wwwroot` altında tutulmayacak. Deployment sırasında yüklenen dosyaların silinmemesi sağlanacak.
- Veritabanında dosyanın kendisi değil, yalnızca metadata tutulacak: dosya adı, fiziksel/göreli yol, dosya tipi, MIME type, dosya boyutu, ilgili ürün/modül kaydı, yüklenme tarihi.
- Dosya işlemleri **storage abstraction** üzerinden yürütülecek; Application ve Presentation katmanları doğrudan `File`, `Directory` veya fiziksel disk yolu kullanmayacak.
- İlk storage implementasyonu yerel dosya sistemi olacak; ileride Blob Storage, S3, MinIO veya Network Storage eklenebilmesi mümkün olacak şekilde tasarlanacak. **Henüz belirli bir bulut sağlayıcısı seçilmeyecek.**
- Dosya yolları kod içine sabit yazılmayacak, configuration üzerinden yönetilecek.
- Doküman Madde 35.4'teki klasör yapısı korunacak: `/products/{urunKodu}/face`, `/render`, `/lifestyle`, `/documents`, `/certificate`, `/catalog`.
- Dosya yüklemelerinde uzantı, MIME type, boyut ve içerik doğrulaması uygulanacak (Madde 31).
- Dosya isimlendirme standardı bu ADR'de **kesinleşmedi** — dokümandaki kurallara göre ayrıca netleştirilecek (mevcut açık konu, bkz. aşağıdaki liste #8).
- WebP/AVIF dönüşümü, responsive image üretimi ve CDN kullanımı bu kararla **otomatik uygulanmış sayılmaz**; bunlar ayrı bir alt karar/task olarak ele alınacak.

**Bilinen Risk (ADR'de kayıtlı):** Çoklu instance deployment yapılırsa (yatay ölçekleme/load-balancing), yerel dosya sistemi her instance'ın kendi diskini görmesi nedeniyle **yetersiz kalabilir** (bir instance'a yüklenen dosya diğerinde görünmeyebilir). Bu risk bilinçli olarak kabul edilmiştir; storage abstraction sayesinde ileride Blob/S3/MinIO/Network Storage'a geçiş, Application/Presentation katmanları değiştirilmeden yapılabilecektir.

**Sebebi:** Mevcut faz için en sade başlangıç noktası; storage abstraction ile ADR-002/ADR-004'ün "genişletilebilir, yeniden yazım gerektirmeyecek katman ayrımı" ilkesi korunur.

**Alternatifleri:** Blob/Object Storage (bu fazda seçilmedi, ancak abstraction sayesinde ileride eklenebilir bırakıldı), Network Storage (reddedildi — hosting altyapısı netleşmeden spekülatif), Veritabanında binary saklama (reddedildi — Karar #5 analizinde performans/ölçek gerekçesiyle).

**Avantajları:** En sade kurulum, ek bulut bağımlılığı/maliyeti yok, abstraction sayesinde ileride teknoloji değişikliği admin modüllerini yeniden yazmayı gerektirmez.

**Dezavantajları:** Çoklu instance senaryosunda yetersiz kalabilir (yukarıdaki risk notu).

**Dokümandaki Dayanağı:** Madde 35.4 (dosya sistemi veya blob storage — ikisi de dokümanda mevcut; klasör yapısı birebir dokümandan), Madde 31 (file upload doğrulaması), Madde 18.2/18.3 ve Madde 37.4 (isimlendirme standardı — açık konu olarak kalıyor).

**Durum:** Onaylandı (kullanıcı, 17.07.2026). Dosya isimlendirme standardı ile WebP/AVIF/responsive image/CDN alt kararı bilinçli olarak açık bırakılmıştır.

---

## ADR-007 — Çoklu Dil Veri Modeli: Merkezi Translations Yaklaşımı (Task 0.2 / Karar #6)

**Karar:** Çoklu dil verileri, dokümanın Madde 36.1'de tanımladığı **merkezi Translations** yaklaşımıyla (EntityType + EntityId ilişkisi) modellenecektir. Entity-bazlı ayrı çeviri tabloları (`ProductTranslation`, `CategoryTranslation` vb.) bu aşamada tercih edilmeyecektir. Dokümandaki bu tanım, alternatif bir teknik tasarım lehine kavramsal özet olarak yorumlanmamıştır — kesin bir şema kararı olarak kabul edilmiştir.

**Kesin kapsam:**
- Merkezi bir çeviri veri modeli kullanılacak; çeviri kaydı ilgili içerik türü ve içerik kimliğiyle (EntityType/EntityId) ilişkilendirilecek.
- Desteklenen diller: TR, EN, DE, FR, ES, AR, RU.
- Dil kayıtları yönetilebilir ve aktif/pasif durumu takip edilebilir olacak; ayrıntılı alanlar entity tasarımı sırasında yalnızca doküman gereksinimlerine göre belirlenecek.
- Panel, her içerik ve dil için çevirinin mevcut veya eksik olduğunu tespit edebilecek; eksik çeviri raporları ve dashboard uyarıları bu merkezi yapı üzerinden desteklenecek.
- Çoklu dil gerektirmeyen alanlar Translations yapısına taşınmayacak (kapsam Madde 28.2'deki alan listesiyle sınırlı).
- `EntityType` değerleri kontrolsüz serbest metin olarak uygulamanın farklı yerlerinde tekrar tekrar yazılmayacak; temsil biçimi (enum/sabit/başka yöntem) sonraki teknik tasarım task'ında belirlenecek.

**Bilinen Risk (ADR'de kayıtlı):** `EntityId` ilişkisi polimorfik olduğu için klasik foreign key kısıtları sınırlı olabilir. Bu nedenle silinen içeriklere ait **yetim (orphan) çeviri kayıtlarının** nasıl temizleneceği, entity ve servis tasarımında ayrıca ele alınacaktır — bu ADR bir temizleme stratejisi belirlemez.

**Bu ADR'nin KESİNLEŞTİRMEDİĞİ konular (nihai veri modeli tasarım task'ına bırakıldı, Madde 28.2 incelenerek karara bağlanacak):**
- `FieldName` sütununun kesin yapısı.
- `Value` sütununun kesin tipi ve uzunluğu.
- Her alan için ayrı kayıt mı, bir dildeki tüm alanların tek kayıtta mı tutulacağı.
- `EntityType`'ın enum, sabit veya başka bir yöntemle temsili.
- Unique index ve constraint ayrıntıları.

**Fallback Davranışı Alt Kararı — Durum: Gelecek Faz / Karar Bekleniyor.** Fallback davranışı (Madde 28.3'ün kendi sunduğu üç seçenek: TR göster / boş bırak / o dilde yayınlama) bu fazda **kesinleştirilmemiştir**. Gerekçe: Madde 28.3 bunu açıkça "Karar Bekleniyor" bırakmış; fallback, esas olarak gelecekteki public web sitesinin içerik gösterim davranışıdır ve ADR-002 gereği mevcut teslimat kapsamının dışındadır. Bu fazda Yönetim Paneli **fallback uygulamayacak**; yalnızca eksik çevirileri gösterecek, çeviri durumunu takip edecek, eksik çeviri raporu üretecek ve dashboard uyarılarını sağlayacaktır.

**Sebebi:** Doküman Madde 36.1'in açık tanımı; merkezi yapının Madde 17.2/41'deki "çeviri durumu takibi, eksik çeviri raporları, dashboard uyarıları" gereksinimlerini doğal desteklemesi.

**Alternatifleri:** Entity-bazlı ayrı çeviri tabloları (A2), dil-bazlı sütunlar (A3), JSON sütun (A4) — hepsi reddedildi; gerekçe: doküman Madde 36.1'in açık tanımı esas alınmıştır.

**Avantajları:** Dokümanla birebir örtüşme; yeni çoklu-dil alan eklemek migration gerektirmez; merkezi raporlama (eksik çeviri) doğal desteklenir.

**Dezavantajları/Risk:** Tip güvenliği eksikliği (nihai şema task'ında ele alınacak); polimorfik FK sınırlaması ve yetim kayıt riski (yukarıda kayıtlı).

**Dokümandaki Dayanağı:** Madde 36.1 (Translations — Languages, EntityType, EntityId), Madde 28 / 28.2 / 28.3, Madde 17.2, Madde 41.

**Durum:** Onaylandı (kullanıcı, 17.07.2026). Nihai şema detayları ve fallback davranışı bilinçli olarak açık bırakılmıştır.

---

## ADR-008 — Bayi/Showroom Veri Modeli: Tek `Dealer` Entity + Category Ayrımı (Task 0.2 / Karar #7)

**Karar:** Bayi ve Showroom **aynı ana veri modeli** (`Dealer`) ve **aynı panel modülü** ("Bayi/Showroom Yönetimi") altında yönetilecektir. Ayrım `Category` alanıyla (Bayi, Showroom) yapılacaktır. Ayrı ve bağımsız bir `Showroom` ana entity'si veya ayrı bir panel modülü **oluşturulmayacaktır**.

**Kesin kapsam:**
- Ana entity: `Dealer`. Listeleme/filtreleme `Category` ile yapılacak; form alanları `Category` değerine göre koşullu gösterilebilir.
- Mevcut 212 kayıt aynı ana yapı içinde yönetilecek. Kategorisiz (17) kayıtların nasıl ele alınacağı bu ADR'de **kesinleşmedi** — veri aktarımı task'ında ayrıca karara bağlanacak.

**Önemli mimari düzeltme (bu ADR'nin asıl ilkesi):** "Tek `Dealer` entity kullanılması", showroom'a ait **bütün** verilerin tek tabloda nullable sütun olarak tutulacağı anlamına **gelmez**. Alanların veri şekline göre ayrım yapılacaktır:
- Tekil/basit showroom özellikleri — gerçekten dokümanda gerekli olduğu kesinleşirse — `Dealer` üzerinde tutulabilir.
- **Galeri görselleri** gibi birden fazla kayıt içeren veriler, `Dealer`'a sütun olarak değil, **ilişkili görsel/medya yapısında** tutulmalıdır.
- **Çalışma saatleri**'nin tek metin alanı mı yoksa gün-bazlı bir yapı mı olacağı, ilgili modül veri modeli task'ında ayrıca değerlendirilecektir.
- **Randevu talep formu** sadece bir aç/kapat özelliğiyse ayrı bir yapı değerlendirilebilir; ancak gerçek kullanıcı randevu talepleri gerekiyorsa, bunlar `Dealer` tablosunda sütun olarak tutulmayacak — **ayrı operasyonel kayıtlar** olarak modellenmesi gerekecektir.
- Dokümanda kesinleşmeyen showroom alanları, sırf ileride kullanılabilir düşüncesiyle şimdiden entity'ye eklenmeyecektir.

**Bu fazda showroom özellikleri hakkında (AÇIK — bu ADR'de kesinleşmedi):** Madde 25.2 ve 26'da "eklenebilir" şeklinde belirtilen galeri, çalışma saatleri ve randevu talep formu **otomatik olarak zorunlu kabul edilmemiştir**. Bu alanların bu teslimatta uygulanıp uygulanmayacağı, Bayi/Showroom modülü task'ında doküman birlikte incelenerek kesinleştirilecektir. Şimdiden kullanılmayan kolon, entity veya tablo oluşturulmayacaktır.

**GÜNCELLEME (Task 14, 19.07.2026) — yukarıdaki açık noktalar kesinleşti:**
- **Galeri görselleri, çalışma saatleri, randevu talep formu, açıklama, sıralama, görsel/logo:** Bu fazda **uygulanmadı**. Gerekçe: Madde 25.1'in gerçek veri modeli tablosu (`DealerId, Name, City, District, Address, Phone, Fax, Email, Latitude, Longitude, Category, Region, RegionName, Status`) bu alanların hiçbirini içermiyor — yalnızca Madde 26'nın "showroom sayfaları... yer alacaktır" ifadesinde (aynı zamanda modül ayrımını da "Karar Bekleniyor" bırakan aynı paragrafta) geçiyorlar. Bu, dokümanın **public site anlatımı** (bu fazın kapsamı dışı, ADR-001/002/009) olup admin panelinin CRUD veri modelini bağlamıyor. Galeri gerekiyorsa (ADR-008'in ilkesi gereği) ayrı bir `DealerImage` ilişkili tablo olarak eklenmeli — bu fazda gerek görülmedi.
- **Translation kullanılmadı:** Madde 25.1 tablosundaki hiçbir alan Product/Blog/Proje tablolarının aksine "(multi-lang)" işaretli değil — `Dealer` **Translation altyapısını hiç tüketmeyen ilk CMS modülü**. `EntityType.Dealer` (Task 3.1B'den beri rezerve) bu task'ta da tüketilmedi, rezerve kalmaya devam ediyor.
- **SEO alanları eklenmedi:** Madde 25.1'de SeoUrl/MetaTitle/MetaDescription yok. Madde 27.2'nin "Bayi: /{dil}/satis-noktalari/{il}/{ilce}" satırı da per-kayıt özel bir slug değil, City+District'ten **programatik türetilen** bir public-site URL deseni — bu, per-entity SEO alanı ihtiyacını ortadan kaldırıyor.
- **Kategorisiz (17) kayıt kararı:** `Category` alanı **nullable** yapıldı (`DealerCategory?`) — yeni bir "Unclassified" enum üyesi icat edilmedi, mevcut projenin nullable-FK deseniyle (BlogCategoryId?/NewsCategoryId? vb.) tutarlı. Import sırasında reddetme veya manuel inceleme kuyruğu gibi ek bir iş akışı bu fazda kurulmadı (Excel Import ayrı backlog maddesi #17).
- **Domain:** `Dealer` (Name, City zorunlu; District/Address/Phone/Fax/Email/Region/RegionName opsiyonel; Latitude/Longitude nullable decimal(9,6), service seviyesinde -90..90/-180..180 aralık + "ikisi birlikte veya hiçbiri" kuralı; Category nullable enum; IsActive bool + ToggleActive — Category/Collection/Banner deseni).
- **RBAC:** Madde 30 Bayi/Showroom satırı **literal** uygulandı: Admin=Tam, İçerik Editörü=—, SEO Editörü=—, Ürün Yöneticisi=— — projedeki **ilk salt-Admin modül** (diğer tüm modüllerde en az bir rol salt-görüntüleme yetkisine sahipti).
- **Durum:** ADR-008 artık **tamamen kapalı** — hiçbir açık nokta kalmadı.

**Sebebi:** Madde 17.2 panelin kendi modül tablosunda "Bayi/Showroom Yönetimi"ni tek modül sayıyor; Madde 25.1'in kendi veri modeli zaten `Category`'yi tek tabloda bir alan olarak tanımlamış; 8 showroom kaydı için ayrı bir ana entity kurmak YAGNI'ye aykırı olurdu. Ancak "her şeyi Dealer'a nullable sütun olarak ekle" yaklaşımı da veri şekline duyarsız bir basitleştirme olurdu — bu nedenle alan bazında ayrım ilkesi benimsenmiştir.

**Alternatifi:** Ayrı `Showroom` ana entity'si — reddedildi (Madde 17.2 ile çelişir, 8 kayıt için YAGNI'ye aykırı, ekstra ilişki/migration karmaşıklığı).

**Avantajları:** Tek modül/tek CRUD ekranı; mevcut veri kolay taşınır; veri şekline duyarlı ayrım sayesinde "her şey nullable sütun" anti-pattern'inden kaçınılır.

**Dezavantajları/Risk:** Kategorisiz 17 kaydın ele alınışı ve showroom-özel alanların bu fazda kapsamda olup olmadığı henüz açık (yukarıda işaretli).

**Dokümandaki Dayanağı:** Madde 17.2, Madde 25, 25.1, 25.2, Madde 26.

**Durum:** Onaylandı (kullanıcı, 17.07.2026). Kategorisiz kayıtların ele alınışı ve showroom-özel alanların (galeri/çalışma saatleri/randevu formu) bu fazda uygulanıp uygulanmayacağı bilinçli olarak açık bırakılmıştır.

---

## ADR-009 — Public Site Entegrasyon Sınırı: Katman Ayrımı Yeterli, Ek İskelet Yok (Task 0.2 / Karar #8)

**Karar:** Bu fazda public site ile yönetim paneli arasındaki gelecekteki entegrasyon için **ek bir teknik iskelet, proje, DTO, contract, interface veya senkronizasyon altyapısı oluşturulmayacaktır**.

**Kesin kapsam — Oluşturulmayacaklar:**
- Public site için ayrı Web API projesi, boş Presentation projesi.
- Kullanılmayan Contracts veya Shared DTO projesi; "ileride kullanılabilir" diye hazırlanan public DTO modelleri.
- Public site endpoint'leri (tasarım/isimlendirme dahil).
- Public site'ın panel ile aynı veritabanını paylaşacağı, ayrı bir veritabanı/read-replica/ETL/senkronizasyon mimarisi kullanacağı veya aynı Infrastructure katmanını kullanacağı — bunların hiçbiri şimdiden varsayılmayacak/kesinleştirilmeyecek.

**Bu fazda genişletilebilirlik için yeterli kabul edilen sınır (kesinleşen ilkeler):**
- Domain katmanı Presentation katmanından bağımsız olacak.
- Application katmanı, admin MVC Controller'larından bağımsız iş kuralları ve use-case'leri içerecek.
- Infrastructure bağımlılıkları soyutlamalar üzerinden kullanılacak.
- MVC Controller'ları yalnızca Application katmanını çağıracak (ADR-003'ün tekrarı/somutlaşması).
- Admin'e özgü sunum modelleri ve davranışlar Domain katmanına taşınmayacak.
- Gelecekte yeni bir Presentation katmanı eklenebilmesini engelleyecek doğrudan bağımlılıklar oluşturulmayacak.

**Önemli ilke:** "Gelecekte public site eklenebilir" düşüncesi, bu fazda kullanılmayan kod yazmak için gerekçe değildir. YAGNI ilkesi uygulanacaktır. Public site'ın veriye API üzerinden mi, ortak Application katmanı üzerinden mi, ayrı bir okuma modeliyle mi erişeceği — **gelecek fazın kendi gereksinim analizinde** belirlenecektir; bu analizde ayrıca ele alınacak konular: uygulama tipi, veri erişim yöntemi, API ihtiyacı, DTO/contract yapısı, veritabanı paylaşımı/ayrımı, cache/CDN yaklaşımı, SEO/performans gereksinimleri, trafik/ölçekleme beklentileri.

**Sebebi:** YAGNI; ADR-001/ADR-002/ADR-003'ün mantıksal devamı; public site'ın gerçek trafik/ölçek/SEO gereksinimleri henüz analiz edilmediği için erken mimari kilitlemenin yanlış tahmin riski taşıması.

**Alternatifleri:** Şimdiden genişletilebilirlik iskeleti (Contracts/DTO projesi), ayrı DB/ETL mimarisi varsayımı, aynı Infrastructure paylaşımı varsayımı — hepsi reddedildi (spekülatif kod, dokümanda dayanağı yok, erken kilitleme riski).

**Avantajları:** Spekülatif kod üretilmez; gelecekteki gerçek ihtiyaca göre doğru mimari seçilebilir; ADR-002'nin "gelecekte kullanılabileceği düşünülerek yazılan kod" yasağıyla tam tutarlı.

**Dezavantajları:** Public site fazı başladığında ilk kurulum işi o zaman yapılacaktır — ADR-001'de zaten kabul edilmiş, kaçınılmaz bir gelecek-faz maliyeti.

**Dokümandaki Dayanağı:** Yok — doküman public site/panel entegrasyon sınırı ayrımını hiç içermiyor (tek bütün proje tasavvur ediyor). Bu karar, ADR-001/002/003'ün mantıksal sonucudur.

**Durum:** Onaylandı (kullanıcı, 17.07.2026).

---

## ADR-010 — SAP Entegrasyonu: Tamamen Faz 2, Faz 1'de Hiçbir Hazırlık Yok (Task 0.2 / Karar #9)

**Karar:** SAP entegrasyonu **tamamen Faz 2** kapsamındadır. Bu fazda SAP entegrasyonuna yönelik **hiçbir** tahmini veri modeli, endpoint, DTO, servis veya senkronizasyon altyapısı oluşturulmayacaktır.

**Kesin kapsam — Oluşturulmayacaklar:**
- SAP API Controller, SAP DTO modelleri, SAP servisleri, SAP Client.
- `SyncStatus`, `SyncDate`, `SapId`, `ExternalId`, `SapVersion`, `SapState` gibi yalnızca olası SAP entegrasyonu için düşünülen alanlar entity'lere **eklenmeyecek**.
- SAP için event, queue, background worker veya scheduler hazırlanmayacak.
- ICD dokümanı oluşmadan entegrasyon varsayımı yapılmayacak.

**Kesinleşen ilkeler:**
- Entity modelleri yalnızca Faz 1 gereksinimlerine göre tasarlanacak.
- Excel Import bu fazın **tek** veri aktarım yöntemi olacak.
- `ProductCode`, dokümanda tanımlandığı şekliyle (Madde 18.1) ürünün doğal anahtarı olarak kullanılacak; bunun ileride SAP eşleştirmesinde kullanılabilmesi bu fazda **özel bir SAP hazırlığı olarak değerlendirilmeyecek** — bu, ürünün kendi ihtiyacından doğan doğal bir sonuçtur.

**Önemli ilke:** ICD dokümanı kesinleşmeden entegrasyon tahmini yapılmayacaktır. Madde 38.1 ve Madde 40 bu konuda bağlayıcı kabul edilecektir.

**SAP entegrasyonu başladığında (gelecek fazda) yeniden analiz edilecek başlıklar:** API sözleşmeleri, authentication yöntemi, mapping kuralları, hata yönetimi, retry stratejileri, senkronizasyon modeli, çakışma yönetimi, transaction sınırları, logging, monitoring.

**Sebebi:** Madde 40 (SAP API Entegrasyonu açıkça Faz 2'de), Madde 38.1 (ICD toplantı sonrası hazırlanacak), ADR-002 (Backend API sınırı SAP endpoint'lerini zaten hariç tutuyor), YAGNI.

**Alternatifleri:** Tahmine dayalı SAP-hazır alan/yapı eklemek, SAP endpoint iskeleti oluşturmak — reddedildi (ICD yokken doğru tahmin ihtimali düşük, ADR-004/008'in "kesinleşmeyen konular için entity/migration oluşturulmayacak" ilkesine aykırı).

**Avantajları:** Spekülatif kod yok; yanlış tahmine dayalı migration/yeniden yazım riski yok.

**Dezavantajları:** SAP entegrasyonu geldiğinde Product şemasında ek migration gerekecek — Madde 38.2'nin kendi "ICD İçin Açık Sorular" listesinde de kabul edilmiş, kaçınılmaz bir durumdur.

**Dokümandaki Dayanağı:** Madde 5, Madde 38, 38.1, 38.2, Madde 39, Madde 40, Madde 44 Varsayım #3.

**Durum:** Onaylandı (kullanıcı, 17.07.2026).

---

## ADR-011 — Güvenlik Gereksinimlerinin Teknik Karşılıkları (Task 0.2 / Karar #10)

**Karar:** Madde 31'in Rate Limiting, Loglama ve File Upload gereksinimleri için aşağıdaki teknik karşılıklar seçilmiştir.

**Rate Limiting:**
- ASP.NET Core'un **yerleşik Rate Limiting middleware'i** kullanılacaktır; üçüncü parti paket kullanılmayacaktır.
- Login ve kritik yönetim işlemlerinde uygun limitler uygulanacaktır; gerçek limit değerleri ilgili güvenlik task'ında belirlenecektir (bu ADR'de kesinleşmedi).
- Dağıtık (distributed) rate limiting veya Redis entegrasyonu bu fazın kapsamında **değildir**.

**Loglama:**
- **Microsoft `ILogger` soyutlaması** kullanılacak; tüm servisler `ILogger<T>` kullanacak. Log provider olarak **Serilog** kullanılacak, ancak uygulama kodu doğrudan Serilog'a bağımlı olmayacak (soyutlama korunacak).
- Structured logging desteklenecek. Log hedefleri (dosya, SQL, Seq vb.) bu ADR'de kesinleşmedi — deployment kararlarında ayrıca değerlendirilecek.
- Hassas bilgiler loglanmayacak. Güvenlik olayları ve hata kayıtları ayrıştırılabilir şekilde tasarlanacak.

**Dosya Doğrulama:**
- Dosya uzantısı, MIME Type ve **Magic Byte (dosya imzası)** doğrulanacak; maksimum dosya boyutu kontrol edilecek; izin verilen dosya tipleri **whitelist** yaklaşımıyla belirlenecek.
- Antivirus/malware taraması bu fazda **uygulanmayacak**.

**Sebebi:** Madde 31'in Rate Limiting/Loglama/File Upload zorunluluğu; YAGNI (native rate limiting, ek bağımlılık yok); Madde 31'in "kullanıcı işlemleri, hata logları, güvenlik olayları" ayrımını Serilog'un structured logging/sink esnekliğiyle temiz karşılama ihtiyacı; Madde 31'in "içerik doğrulaması" ifadesini gerçekten karşılayan (magic-byte) ama aşırıya kaçmayan (antivirus yok) bir dosya doğrulama seviyesi.

**Alternatifleri:** Üçüncü parti rate limiting paketi (reddedildi — ek bağımlılık), sadece yerleşik `ILogger`+dosya/konsol (reddedildi — structured logging/çoklu hedef esnekliği için Serilog tercih edildi), sadece MIME kontrolü (reddedildi — sahteleme riski), MIME+magic-byte+antivirus (reddedildi — dokümanda dayanağı yok, bu faz için aşırı).

**Avantajları:** Ek bağımlılık minimize edilir (rate limiting); loglama esnek/yapılandırılabilir kalırken kod Serilog'a kilitlenmez (ILogger soyutlaması); dosya doğrulama gerçek güvenlik değeri sağlar (magic-byte sahteci MIME header'ları yakalar).

**Dezavantajları:** Rate limiting'in gerçek limit değerleri ve log hedefleri henüz belirlenmedi (bilinçli olarak sonraki task'lara bırakıldı).

**Dokümandaki Dayanağı:** Madde 31 (Authentication, Rate Limiting, File Upload, Loglama satırları).

**Durum:** Onaylandı (kullanıcı, 17.07.2026). Rate limit değerleri, log hedefleri ve dağıtık rate limiting ihtiyacı bilinçli olarak açık bırakılmıştır.

---

## ADR-012 — Translation ve Language Nihai Şema Kararları (ADR-007'nin Detaylandırılması, Task 3.1A)

**Karar:** ADR-007'de ilke olarak onaylanan merkezi Translations yaklaşımının bıraktığı şema detayları ve ayrı bir Language entity'si aşağıdaki şekilde kesinleştirilmiştir. **Bu ADR, ADR-007'yi değiştirmez veya geçersiz kılmaz — ADR-007'nin bıraktığı açık maddeleri detaylandıran, onu tamamlayan bir devam kararıdır.**

**Kesin kapsam:**

- **FieldName:** Düz `nvarchar`, DB seviyesinde constraint yok. Geçerli alan adları Application katmanında entity-bazlı const string sabitleriyle tanımlanır (örn. `ProductFields.Name`). Yeni çevrilebilir alan eklemek migration gerektirmez.

- **Value:** Tek `nvarchar(max)` sütun. Kısa (Name, Title) ve uzun (Description, Blog body) tüm alan değerleri aynı sütunda tutulur; ayrı kısa/uzun sütun ayrımı yapılmaz.

- **Satır yapısı:** Her (EntityType, EntityId, LanguageId, FieldName) kombinasyonu ayrı bir satırdır. JSON/tek-satır-çoklu-alan modeli kullanılmaz — eksik çeviri raporlaması (Madde 17.2/41) alan bazlı granülerlik gerektirir.

- **EntityType:** C# enum olarak tanımlanır (Domain katmanında, framework bağımsız). EF Core'da **açık bir `ValueConverter<EntityType, string>`** ile string'e dönüştürülür — varsayılan `HasConversion<string>()` / `enum.ToString()` davranışına **kesinlikle bağımlı kalınmaz**. Enum üyesi ile DB'deki sabit string değer birbirinden tamamen ayrıştırılır:
  - `EntityType.Product` ↔ `"PRODUCT"`
  - `EntityType.Category` ↔ `"CATEGORY"`
  - `EntityType.Blog` ↔ `"BLOG"`
  - (yeni modüller eklendikçe aynı desenle genişler)

  Kurallar:
  - Converter **yalnızca Infrastructure katmanında** yaşar; Domain'e EF Core bağımlılığı taşınmaz.
  - Enum → string ve string → enum dönüşümleri açık (elle yazılmış switch/sözlük) olarak tanımlanır.
  - Tanımsız/eksik bir mapping durumunda **sessiz fallback yapılmaz**; açık ve anlaşılır bir exception fırlatılır.
  - Yeni bir `EntityType` üyesi eklendiğinde, enum tanımı ve converter **birlikte** güncellenir.

- **Unique index (Translation):** `(EntityType, EntityId, LanguageId, FieldName)` — aynı alanın aynı dilde iki kez çevrilmesini DB seviyesinde engeller.

- **Language entity:** `Id` (surrogate PK), `Code` (örn. "TR" — ayrı bir **unique index** ile korunur, iş anahtarı olarak kullanılır ama FK hedefi değildir), `Name`, `IsActive`, `DisplayOrder`.

- **Translation ↔ Language ilişkisi:** `Translation.LanguageId` → `Language.Id` klasik (surrogate) foreign key. `Translation` tablosunda ayrıca `LanguageCode` sütunu **tutulmaz** — dil bilgisine ihtiyaç duyulduğunda `Language` navigation property veya join üzerinden erişilir.
  - Silme davranışı: `ON DELETE RESTRICT` — çevirisi olan bir `Language` kaydı fiziksel olarak silinemez. Yönetim ekranında dil kullanım dışı bırakılmak istendiğinde `IsActive = false` yapılır, hard-delete yapılmaz.

- **Yetim (orphan) çeviri kaydı temizliği:** DB trigger veya background job kullanılmaz. Her entity'nin silme işlemiyle **aynı transaction/scope içinde**, Application katmanındaki ilgili servis (örn. `ITranslationService.DeleteTranslationsForAsync(entityType, entityId)`) çağrılarak yönetilir.

- **Migration:** `Language` ve `Translation`, Task 3.1B'de **tek migration**da birlikte oluşturulur (FK bağımlılığı zaten Language'ın önce var olmasını gerektiriyor; EF Core CREATE TABLE sırasını kendisi topolojik olarak çözer). Dil verisi (TR/EN/DE/FR/ES/AR/RU) migration dosyasına gömülmez; `IdentitySeeder` deseniyle birebir aynı şekilde, `dotnet ef database update` sonrası ayrı bir seeder ile eklenir.

**Sebebi:** ADR-007'nin açık bıraktığı 6 şema detayının kapatılması; Language modülünün (Madde 17.2) CRUD/aktif-pasif/sıralama ihtiyacı; enum-string ve FK tasarımlarında veri bütünlüğünün mimari garantiye (disiplin kuralına değil) bağlanması.

**Alternatifleri:** FieldName için DB constraint (reddedildi — migration bağımlılığı), Value için ayrı kısa/uzun sütun (reddedildi — nullable-sütun anti-pattern'i), JSON tek-satır model (reddedildi — raporlama gereksinimiyle çelişir), EntityType için const string (değerlendirildi, enum + açık converter lehine reddedildi), EntityType için varsayılan `HasConversion<string>()`/`enum.ToString()` (reddedildi — üye rename riskiyle veri sessizce bozulabilir), LanguageId yerine Language.Code üzerinden doğal anahtar FK (değerlendirildi — EF Core idiomu, migration basitliği ve maintainability gerekçesiyle Id lehine reddedildi), iki ayrı migration (reddedildi — FK bağımlılığı ve tek özellik bütünlüğü).

**Avantajları:** DB seviyesinde dil ilişkisi bütünlüğü (standart, düşük-riskli EF Core deseni); EntityType'ta compile-time typo güvenliği + mimari olarak garanti altına alınmış enum-string ayrışması (disipline değil, koda dayalı); eksik çeviri raporlaması için gerekli granülerlik; migration'sız yeni çevrilebilir alan ekleme.

**Dezavantajları/Bilinen Riskler:** Polimorfik `EntityId` nedeniyle DB seviyesinde gerçek FK yok — bütünlük tamamen Application disiplinine bağlı (15 modülün her birinde silme akışında hatırlanmalı); her modülde tekrarlanan sorgu/mapping/join karmaşıklığı (generic translation loader abstraction'ı Task 3.1B'de ayrıca değerlendirilecek); `Translation`, en yoğun trafikli tek tablo olur; `LanguageId` FK'si ham SQL/debugger'da doğrudan okunamaz, dil bilgisi için join gerekir (proje zaten doğrulama sorgularında join alışkanlığına sahip, ek bir yük değil); EntityType converter'ın enum ile senkron tutulması code review disiplini gerektirir (mitigasyon: eksik mapping'de exception, sessiz bozulma yok).

**Dokümandaki Dayanağı:** Madde 36.1, Madde 17.2 (Dil Yönetimi modülü), Madde 41.

**Durum:** Onaylandı (kullanıcı, 19.07.2026). ADR-007'yi tamamlar, değiştirmez. Yalnızca şema kararı — implementasyon (entity/DbContext/configuration/migration) Task 3.1B'de ayrıca yapılacak.

---

## ADR-013 — Dosya Depolama Abstraction'ı ve Ürün Görseli Modeli (ADR-006'nın İlk Somutlaşması, Task 5.1)

**Karar:** ADR-006'nın ilke olarak onayladığı "Yerel Dosya Sistemi + storage abstraction" yaklaşımı, ilk kez Ürün Görselleri modülünde (Backlog #8) somut olarak koda geçirilmiştir. Bu ADR, ADR-006'yı değiştirmez/geçersiz kılmaz — onu tamamlayan, gelecekteki tüm dosya/görsel içeren modüllerin (Blog kapak görseli, Banner, Referans Proje galerisi, Katalog/Doküman PDF'leri) doğrudan tekrar kullanabileceği somut bir örnek/ilke oluşturur.

**Kesin kapsam:**

- **Katman ayrımı:** `Application/Storage/IFileStorageService.cs` — yalnızca `SaveAsync(relativeFolder, Stream, fileName)`, `Delete(relativeFilePath)`, `Exists(relativeFilePath)`. Application ve Domain, ASP.NET Core'un `IFormFile` tipine **hiç bağımlı değildir** — yalnızca `System.IO.Stream` (BCL) kullanılır. `IFormFile` yalnızca Presentation katmanında (Controller) açılır, stream/dosya adı/content-type/length gibi sade veriler Application'a taşınır.
- **Implementasyon:** `Infrastructure/Storage/LocalFileStorageService.cs` — fiziksel kök `wwwroot/uploads`, `IWebHostEnvironment.WebRootPath` üzerinden çözülür (Infrastructure zaten `Microsoft.AspNetCore.App` FrameworkReference'ına sahip — Task 1.2A'dan beri). Path traversal koruması hem `SaveAsync` (hedef klasör dışına taşma engeli) hem `Delete`/`Exists` (yalnızca `uploads/` kökü altındaki yollar kabul edilir) için ayrı ayrı uygulanmıştır.
- **Klasör yapısı (Madde 35.4 + ADR-006 birebir uygulandı):** `/uploads/products/{ProductCode}/{gorselTipiKucukHarf}/{guid}.{uzanti}` — örn. `/uploads/products/55018167RP/face/3f2a1c9e....jpg`. Doküman `{urunKodu}` diyor; `ProductId` değil `ProductCode` kullanıldı (ADR'ye literal sadakat). Bilinçli kabul edilen küçük risk: `ProductCode` sonradan değiştirilirse, o ana kadar yüklenmiş görsellerin klasör adı eski kodu taşımaya devam eder — bu **veri kaybına veya kırık linke yol açmaz** (DB'de tam web-relative yol saklanır, sonradan yeniden hesaplanmaz), yalnızca kozmetik bir tutarsızlıktır; yeni yüklenen görseller güncel koda göre yeni bir klasöre gider.
- **İzin verilen formatlar:** yalnızca `.jpg`, `.jpeg`, `.png`, `.webp` (whitelist). SVG, GIF ve diğer tüm formatlar reddedilir (SVG script içerebileceği için özellikle dışlandı — doküman da istemiyor).
- **Maksimum dosya boyutu:** 5 MB/dosya — doküman bir sınır belirtmediği için MVP kararı olarak alınmıştır (küçük, geri dönüşü kolay bir karar; ihtiyaç halinde tek bir sabit değiştirilerek büyütülebilir).
- **Güvenli dosya adlandırma:** Kullanıcının orijinal dosya adı **hiçbir zaman** fiziksel yol olarak kullanılmaz. Uzantı whitelist'ten doğrulanır, dosya adı her zaman `{Guid.NewGuid():N}{uzanti}` ile yeniden üretilir — çift uzantı, path traversal ve isim çakışması riskleri kökten ortadan kalkar.
- **İçerik doğrulama (ADR-011'in "magic-byte" ilkesinin ilk gerçek uygulaması):** Uzantı whitelist + uzantı↔MIME tutarlılık kontrolü + dosyanın ilk baytlarının (JPEG: `FF D8 FF`, PNG: 8-bayt PNG imzası, WEBP: `RIFF....WEBP`) beyan edilen formatla eşleşmesi. Üçüncü parti kütüphane **eklenmedi** — saf BCL byte karşılaştırması yeterli görüldü (ADR-011'in "aşırıya kaçmayan doğrulama" ilkesiyle tutarlı; tam image-decode/re-encode yapılmıyor).
- **`ProductImage` modeli:** `Id`, `ProductId`/`Product` (FK, `Cascade`), `ImageType` (enum: `Render`/`Face`/`Lifestyle`/`Texture`/`Detail` — Madde 18.2'nin 5 görsel tipi birebir), `FilePath` (nvarchar(500), web-relative), `IsPrimary` (bool), `DisplayOrder` (int). `Product` entity'sinde ayrı bir `ImagePath` alanı **yoktu** (Task 5'te bilinçli olarak eklenmemişti — Madde 18.2'nin çoklu-görsel modelini doğru öngörmüştü); bu nedenle "eski alanı kaldırma" riski hiç oluşmadı, tek doğruluk kaynağı baştan `ProductImage` tablosudur.
- **Ana görsel (IsPrimary) garantisi:** Application katmanında (ilk yüklenen görsel otomatik ana görsel, ana görsel silinirse en düşük `DisplayOrder`'lı görsel devralır, `SetPrimary` eskisini otomatik kaldırır) **VE** DB seviyesinde SQL Server **filtered unique index** (`WHERE [IsPrimary] = 1`) ile çift koruma. Bu, projenin ilk filtered unique index kullanımıdır — aşırı karmaşıklık sayılmadı, tek satırlık Fluent API konfigürasyonu.
- **Fiziksel dosya ↔ DB tutarlılığı (tam atomik olamaz, telafi mantığıyla yönetildi):** Yükleme: önce dosya diske yazılır, sonra DB kaydı oluşturulur; DB adımı başarısız olursa yeni yazılan fiziksel dosya geri silinir (compensating action) ve hata loglanır (`ILogger<ProductImageService>` — bu, Application katmanında `ILogger<T>` kullanımının **ilk örneğidir**, bunun için `Microsoft.Extensions.Logging.Abstractions` paketi Application.csproj'a eklendi). Silme: önce DB kaydı silinir (kaynak-doğruluğu DB'dir), sonra fiziksel dosya silinir; fiziksel silme başarısız olursa hata loglanır ama kullanıcıya **başarı** dönülür (DB tutarlı, artık dosya orphan kalabilir — kabul edilen küçük risk, dağıtık transaction mekanizması eklenmedi).
- **Ürün silme entegrasyonu:** `ProductService.DeleteAsync`, `Product` satırını silmeden önce `ProductImageService.DeleteAllForProductAsync` çağırır (DB kaydı + fiziksel dosya temizliği garanti edilir). DB'de ayrıca `ProductId` FK'si `Cascade` olarak tanımlıdır (defense-in-depth — servis katmanı bypass edilse bile orphan `ProductImage` satırı kalmaz; fiziksel dosya temizliği yalnızca servis çağrısıyla garanti edilir).
- **RBAC:** Madde 30'un Ürün Yönetimi için istediği alan-seviyeli kısmi yetkiler (İçerik Editörü'nün görsel düzenleyebilmesi) bu modülde de **uygulanmadı** — Task 5'teki kararla tutarlı olarak `ProductImageController`'ın tüm action'ları yalnızca Admin+Ürün Yöneticisi'ne açıktır. Gerekçe ve kapsam Task 5 kapanış raporunda zaten kayıtlı; bu, ayrı bir "alan-seviyeli RBAC" backlog maddesine bırakılmıştır.

**Sebebi:** ADR-006'nın "storage abstraction, ileride Blob/S3/MinIO'ya geçiş mümkün" ilkesinin ilk gerçek sınavı; ADR-011'in dosya doğrulama gereksinimlerinin (uzantı+MIME+magic-byte+whitelist) ilk gerçek uygulaması; Madde 18.2/35.4'ün somut gereksinimleri.

**Alternatifleri:** Generic/merkezi medya kütüphanesi (tüm modüllerin ortak kullanacağı `Media`/`Attachment` tablosu) — değerlendirildi, reddedildi (YAGNI; henüz yalnızca tek bir tüketici — Product — var, ihtiyaç gerçekten ortaya çıktığında refactor edilebilir, şimdiden soyutlamak spekülatif olurdu). `ProductId` yerine `ProductCode` klasör anahtarı — tercih edildi (ADR-006'ya literal sadakat, risk analiz edildi ve kabul edilebilir bulundu). Ayrı bir `IImageValidationService`/üçüncü parti image-processing kütüphanesi — reddedildi (ADR-011'in "aşırıya kaçmayan" ilkesi, MVP kapsamı).

**Avantajları:** Application/Domain hâlâ ASP.NET Core'a bağımlı değil (yalnızca Infrastructure `IWebHostEnvironment` kullanıyor); ileride Blob/S3 geçişi yalnızca `LocalFileStorageService`'in değiştirilmesini gerektirir; DB+dosya tutarlılığı hem uygulama mantığı hem DB constraint ile çift güvenceli; sonraki görsel/dosya içeren modüller (#9 Katalog/Doküman, Blog kapak görseli, Banner, Referans Proje galerisi) bu deseni doğrudan örnek alabilir.

**Dezavantajları/Bilinen Riskler:** Fiziksel dosya silme başarısız olursa orphan dosya diskte kalabilir (loglanır, otomatik temizlik/garbage-collection mekanizması yok — kabul edilen risk); `ProductCode` değişikliğinde klasör adı eskimiş kalır (yukarıda gerekçeli, veri kaybı yok); tam atomiklik yok (dosya+DB iki ayrı sistem, telafi mantığıyla yönetiliyor, dağıtık transaction yok).

**Dokümandaki Dayanağı:** Madde 18.2 (Ürün Görselleri, 5 görsel tipi), Madde 35.1 (katmanlı mimari), Madde 35.4 (dosya yönetimi klasör yapısı — ADR-006 ile zaten onaylı), Madde 31 (dosya yükleme güvenliği — ADR-011 ile zaten onaylı).

**Durum:** Onaylandı (kullanıcı, 19.07.2026). ADR-006/ADR-011'i tamamlar, değiştirmez. Sonraki dosya/görsel içeren modüllerin bu deseni tekrar kullanması beklenir.

---

## ADR-014 — Katalog/Doküman Veri Modeli, M2M İlişkiler ve Ürün-Sahipli-Olmayan Dosyalar İçin Klasör Yapısı Sapması (Backlog #9)

**Karar:** Madde 24 (Katalog Yönetimi) ve Madde 36.1/36.2 esas alınarak `Document` bağımsız bir entity olarak modellendi; `Product` ve `Collection` ile ilişkisi **many-to-many** (`ProductDocuments`/`CollectionDocuments` junction tabloları — doküman bu isimleri Madde 36.2'de birebir kullanıyor) ve **opsiyoneldir** ("genel seviye" doküman hiçbir ilişkiye sahip olmayabilir, Madde 24: "...ürün, koleksiyon veya genel seviyede ilişkilendirilir"). Bu ADR, ADR-006/ADR-013'ü tamamlar; ADR-013'ün literal `/products/{urunKodu}/...` klasör örneğinin M2M/opsiyonel-ilişkili varlıklar için **yapısal olarak uygulanamayacağını** tespit edip gerekçeli bir sapma tanımlar.

**Kesin kapsam:**

- **Çoklu dil kararı — Translation KULLANILMADI (kritik, doküman-içi tutarlılıkla çözüldü):** Madde 24'ün `Document` tablosu hem `DocumentName` alanını "string(multi-lang)" olarak işaretliyor hem de ayrı bir `Language` (enum, "Dokümanın dili") alanı listeliyor. Madde 18.3'ün dosya isimlendirme standardı ("urunKodu_dokumanTipi_dil.pdf", örn. `55018167_teknik-foy_tr.pdf`) bu görünür çelişkiyi çözüyor: **her fiziksel PDF tek bir dile aittir** — aynı dokümanın TR ve EN sürümü, aynı satırın Translation ile çevrilmiş hali değil, **iki ayrı `Document` satırı/iki ayrı fiziksel dosyadır**. Bu nedenle `Document.Title` **Translation'a taşınmadı**, native `nvarchar(300)` sütun olarak tutuluyor; `Document.LanguageId` (mevcut `Language` entity'sine FK, `Translation.LanguageId` ile aynı desende) satırın hangi dile ait olduğunu belirtiyor. `EntityType.Document` eklenmedi/gerekmedi.
- **İlişki modeli — many-to-many, doğrudan FK DEĞİL:** `Document` üzerinde `ProductId`/`CollectionId` **yok**. `ProductDocument` (ProductId+DocumentId) ve `CollectionDocument` (CollectionId+DocumentId) junction entity'leri var, ikisi de composite unique index ile korunuyor. Bir doküman sıfır, bir veya birden fazla ürüne/koleksiyona bağlanabilir.
- **Silme davranışı (kritik, dokümandan çıkarıldı, tahmin edilmedi):** `ProductDocument.ProductId` ve `CollectionDocument.CollectionId` FK'leri **Cascade** — Product/Collection silindiğinde yalnızca ilişki satırı silinir, **`Document` kaydı ve fiziksel dosya etkilenmez** (aynı dosya başka ürünlere/koleksiyonlara bağlı kalabilir veya zaten "genel seviye" olabilir). `ProductDocument.DocumentId`/`CollectionDocument.DocumentId` FK'leri de Cascade — `Document` açıkça silindiğinde (DocumentController üzerinden) ilişki satırları otomatik temizlenir, fiziksel dosya `DocumentService.DeleteAsync` içinde açıkça silinir (ProductImage'daki telafi-mantığı deseniyle birebir). `Document.LanguageId` FK'si **Restrict** (ADR-012'deki `Translation.LanguageId` ile tutarlı).
- **Klasör yapısı sapması (gerekçeli, ADR-006'yı geçersiz kılmaz):** ADR-006/Madde 35.4 klasör örneği `/products/{urunKodu}/documents, /certificate, /catalog` şeklinde, Product-öncelikli bir hiyerarşi varsayıyordu — bu, ADR-006 yazıldığında (Task 0.2, hiçbir entity tasarlanmadan önce) Document'ın gerçek cardinality'sinin (M2M + opsiyonel) henüz analiz edilmemiş olmasından kaynaklanıyor. Bir doküman birden fazla ürüne VEYA hiçbir ürüne bağlı olabildiği için, `{urunKodu}` bazlı bir klasör anahtarı yapısal olarak doğru değildir (hangi ürünün kodu seçilecek? Ürünsüz/genel dokümanlar nereye gidecek?). Bunun yerine **doküman tipi bazlı, ürün/koleksiyondan bağımsız** bir yapı kullanıldı: `/uploads/documents/{tipSegmenti}/{guid}.pdf` — `tipSegmenti` ∈ {catalog, documents, certificate, reports} (Madde 35.4'ün üç segment adıyla ADR-006 terminolojisine sadık kalındı, `reports` yeni bir segment olarak eklendi — Madde 18.2'de Texture/Detail'in Madde 35.4'ün orijinal 3-segment listesine sonradan eklenmesiyle aynı emsal). Bu, ADR-006'nın **ilkesini** (yerel dosya sistemi, storage abstraction, tip-bazlı segmentasyon, güvenli GUID isimlendirme) tam olarak korur; yalnızca üst-seviye product-code önekini M2M/opsiyonel varlıklar için mantıksal olarak uygulanamaz bulup atlar.
- **Storage abstraction — hiçbir değişiklik gerekmedi:** `Application/Storage/IFileStorageService` ve `Infrastructure/Storage/LocalFileStorageService` (Task 5.1) **birebir aynı haliyle** tekrar kullanıldı — arayüz zaten tamamen generic'ti (`relativeFolder` + `fileName` + `Stream`), image'a özel hiçbir varsayım içermiyordu. Bu, ADR-013'ün "sonraki modüller bu deseni tekrar kullanabilir" öngörüsünü doğruladı.
- **Dosya güvenliği:** Yalnızca `.pdf`/`application/pdf`, `%PDF-` magic-byte imza kontrolü, GUID dosya adı (kullanıcı dosya adı yalnızca `OriginalFileName` metadata alanında), maksimum 20 MB/dosya (dokümanda sınır yok — MVP kararı, ProductImage'ın 5MB'ından daha yüksek çünkü kataloglar/teknik föyler doğal olarak daha büyük PDF'lerdir).
- **DocumentType:** Kapalı liste, doğrudan Madde 24'ün kendi tablosundan: `Catalog` (Katalog), `TechnicalSheet` (Teknik Föy), `Certificate` (Sertifika), `Report` (Rapor). Başka tür (Brochure, MaintenanceGuide vb.) **uydurulmadı**.
- **RBAC — Madde 30'a literal sadakat (Task 5/5.1'den farklı bir karar, gerekçeli):** Doküman bu modül için **açıkça** bir satır içeriyor: Admin=Tam, İçerik Editörü=Yükleme, SEO Editörü=—, Ürün Yöneticisi=Tam. Task 5/5.1'in "alan-seviyeli RBAC'ı sessizce genişletme" ilkesinin aksine, burada **action-seviyeli** bir ayrım (Create/Upload ayrı, Edit/Delete ayrı) doğrudan uygulanabilir olduğu için hayata geçirildi: İçerik Editörü yalnızca `Create` action'ına erişebiliyor (Edit/ToggleActive/Delete yok), SEO Editörü bu modüle **hiç** erişemiyor (diğer modüllerin aksine salt-görüntüleme bile yok). Ayrıca Ürün Yöneticisi'ne "Tam" dendiği için (önceki modüllerin kendi kararı olan "silme yalnızca Admin" kısıtından farklı olarak) **silme yetkisi de verildi**.
- **Yan-bulgu ve düzeltme (bu task sırasında keşfedildi):** `CategoryService.DeleteAsync`/`CollectionService.DeleteAsync`, Task 5'te eklenen `Product.CategoryId`/`CollectionId` (Restrict FK) referanslarını kontrol etmiyordu — hâlâ bir ürün tarafından kullanılan kategori/koleksiyon silinmeye çalışıldığında EF Core değişiklik-izleyicisi hatası (veya taze bir DbContext'te ham DB FK ihlali) ile **çöküyordu**. `IProductRepository`'ye `HasAnyWithCategoryIdAsync`/`HasAnyWithCollectionIdAsync` eklendi, her iki servis de silmeden önce bu kontrolü yapıp anlaşılır bir hata mesajıyla reddediyor artık. Bu, Document modülünün kendi kapsamı dışında ama doğrudan ilişkili bir bütünlük düzeltmesidir — küçük ve geri dönüşü kolay olduğu için onay istenmeden düzeltildi.

**Sebebi:** Madde 24/36.1/36.2'nin M2M ve opsiyonel-ilişki gereksinimlerinin doğru modellenmesi; Madde 18.3'ün dosya-başına-tek-dil kanıtının Translation yaklaşımını mimari olarak yanlış kılması; ADR-006'nın product-code-öncelikli klasör örneğinin M2M cardinality ile yapısal uyumsuzluğu.

**Alternatifleri:** `Document.Title` için Translation/`EntityType.Document` (değerlendirildi, reddedildi — Madde 18.3 kanıtıyla çelişir, gereksiz karmaşıklık ekler). `Document`'ta doğrudan nullable `ProductId`/`CollectionId` (değerlendirildi, reddedildi — Madde 36.1/36.2'nin M2M açıklamasıyla ve "birden fazla ürünle ilişkilendirilebilir" ifadesiyle doğrudan çelişir). `/products/{code}/documents/...` klasör yapısına literal sadakat (değerlendirildi, reddedildi — M2M/opsiyonel ilişki için yapısal olarak tanımsız, hangi ürünün "sahip" sayılacağı belirsiz). Merkezi/generic `IFileStorageService` değişikliği (gerekmedi — arayüz zaten yeterince generic'ti).

**Avantajları:** Doküman kendi başına bir aggregate root, ürün/koleksiyon silme işlemlerinden bağımsız yaşıyor (paylaşılan kataloglar için doğru davranış); Translation kullanılmayarak gereksiz karmaşıklık ve `EntityType` genişletmesi önlendi; storage abstraction'ın gerçekten reusable olduğu ikinci kez kanıtlandı; Category/Collection silme akışındaki gerçek bir bug bu task sayesinde bulundu ve düzeltildi.

**Dezavantajları/Bilinen Riskler:** Klasör yapısı artık ADR-006'nın literal metniyle birebir örtüşmüyor (yalnızca ilkesiyle) — ileride ADR-006 metni bu sapmayı yansıtacak şekilde güncellenebilir, şimdilik ADR-014 ile çapraz referanslanıyor. `Document` silinmeden Product/Collection tarafında "bu dosya hâlâ kullanılıyor mu" farkındalığı yalnızca ilişki sayısı üzerinden (`RelatedProducts`/`RelatedCollections` DTO alanları) admin ekranında görünür — otomatik "orphan doküman" raporu bu task'ta yok (gelecekte SEO/Dashboard task'larında değerlendirilebilir).

**Dokümandaki Dayanağı:** Madde 17.2 (Katalog/Doküman Yönetimi modülü), Madde 18.3 (dosya-başına-tek-dil isimlendirme kanıtı), Madde 20 (koleksiyon-doküman ilişkisi), Madde 24 (Document veri modeli, DocumentType kapalı listesi), Madde 30 (RBAC), Madde 31 (dosya güvenliği), Madde 35.4 (klasör yapısı ilkesi — ADR-006), Madde 36.1/36.2 (M2M ilişki modeli, junction tablo adları).

**Durum:** Onaylandı (kullanıcı, 19.07.2026). ADR-006/ADR-013'ü tamamlar, değiştirmez; ADR-006'nın literal klasör örneğine gerekçeli bir sapma ekler.

---

## ADR-015 — Form Yönetimi Veri Modeli ve Projedeki İlk Gerçek SQL-Seviyesi Pagination/Filtreleme (Backlog #16, Task 15)

**Karar 1 — Tek `FormSubmission` entity + FormType discriminator (Seçenek 1):** Madde 29 (Formlar) yalnızca 3 form türünü somut alan listesiyle tanımlıyor: 29.1 İletişim, 29.2 Request Information/Bilgi Talep, 29.3 Numune Talep. Randevu talep formu (Madde 26, showroom'a özel, ADR-008'de zaten "eklenebilir/Karar Bekleniyor, uygulanmadı" olarak kapatıldı), bayi başvuru formu ve kariyer formu dokümanda somut alan listesiyle tanımlanmadığı için **eklenmedi**. Üç form türünün ortak alanları (FullName/Email/Phone/Company/Message/ConsentAccepted) ile tip-özel alanları (Contact: Subject; RequestInformation: ProductCode/ProductName; SampleRequest: Address/RequestedProduct/Quantity) birkaç skaler alandan ibaret — ayrı detay entity'leri (Seçenek 2) veya dinamik `FormDefinition`/`FormField`/JSON blob (Madde 36.1'in "FormFields" ifadesi yalnızca kavramsal, dinamik form builder UI'ı hiçbir yerde tarif edilmiyor) bu ölçek için haksız karmaşıklık olurdu. Admin panelinin "tek gelen kutusu" ihtiyacı (Madde 17.2: "İletişim ve bilgi talep formları listesi") da tek tabloyu destekliyor.

**Karar 2 — Durum: Status enum yerine IsRead/ReadAt/ProcessedAt:** Madde 17.2 yalnızca soyut "durum takibi" diyor, Blog/News/Product'taki gibi somut değer listesi (Taslak/Yayında/Arşiv vb.) vermiyor. İcat edilmiş bir Status enum yerine, okuma ve işleme aşamalarını nullable zaman damgasıyla temsil eden `IsRead`(bool)+`ReadAt`+`ProcessedAt` kullanıldı (`ProcessedAt` dolu olması "işleme alındı" anlamına gelir) — Banner'ın `PublishStartDate`/`PublishEndDate` nullable-zaman-damgası deseniyle tutarlı, uydurma enum etiketi yok.

**Karar 3 — Translation ve dosya eki KULLANILMADI:** Tüm alanlar kullanıcı tarafından girilen ham veri (ad/e-posta/mesaj/firma/adres) — admin tarafından yönetilen çevrilebilir bir başlık/etiket yok (Dealer'dan sonra Translation'ı hiç tüketmeyen ikinci modül). Madde 29.1/29.2/29.3'ün hiçbirinde dosya/CV/ek alanı yok — `IFileStorageService` kullanılmadı.

**Karar 4 — Public form gönderim endpoint'i bu fazda YAZILMADI:** ADR-001/002/009 bu fazı yalnızca Admin Panel + Backend API ile sınırlıyor, public site kodu (public controller/endpoint dahil) "hiçbir aşamada YAZILMAYACAK" diyor. `FormSubmissionService.CreateSubmissionAsync` Application katmanında hazır ve test edilmiş durumda (gelecekteki public site fazı doğrudan çağırabilir) ama bu task'ta hiçbir `[AllowAnonymous]` controller/action'dan çağrılmıyor. Aynı gerekçeyle e-posta bildirimi (Madde 14.3/17.2/29.2) de kurulmadı — bildirim, var olmayan public gönderim akışının bir sonucu; SMTP/MailKit altyapısı bu task'ın kapsamına şimdiden eklenmedi, public site fazında ele alınacak.

**Karar 5 (asıl yeni mimari desen) — Projedeki ilk gerçek SQL-seviyesi pagination/filtreleme:** Şimdiye kadarki tüm modüller (Category'den News'e) `GetAllAsync()` ile tüm kayıtları çekip Controller/DTO seviyesinde in-memory filtreliyordu — küçük, sınırlı büyüyen veri setleri (kategori sayısı, ürün sayısı sabit) için kabul edilebilirdi. Form kayıtları ise **zamanla sürekli büyüyen** bir veri seti (Madde 17.2, görev talimatı) — bu yüzden `IFormSubmissionRepository.GetPagedAsync(FormSubmissionQuery)` gerçek `IQueryable` tabanlı `.Where()`/`.OrderByDescending()`/`.Skip()`/`.Take()` + ayrı `.CountAsync()` kullanıyor, hiçbir noktada tüm tablo belleğe çekilmiyor. **Bundan sonraki modüller için ilke:** veri seti doğası gereği sürekli büyüyorsa (kullanıcı gönderimleri, log/audit kayıtları vb.) `GetPagedAsync` deseni tercih edilmeli; veri seti operasyonel olarak sınırlıysa (kategori ağacı, ürün kataloğu gibi işletme tarafından yönetilen, öngörülebilir sayıda kayıt) mevcut `GetAllAsync()` + in-memory listeleme deseni yeterli ve daha basittir — otomatik olarak her yeni modülde pagination'a geçilmeyecek.

**Sebebi:** Madde 29'un somut 3 form türü + Madde 17.2'nin "form kayıtları listesi, durum takibi" ifadesi; ADR-001/002/009'un public site sınırı; form verisinin işletme-yönetimli değil kullanıcı-üretimli, dolayısıyla sınırsız büyüyebilen bir veri seti olması.

**Alternatifleri:** Dinamik form builder (`FormDefinition`/`FormField`, JSON blob) — değerlendirildi, reddedildi (doküman hiçbir yerde admin form tasarım ekranı tarif etmiyor, Madde 36.1'in "FormFields" ifadesi tek başına bunu haklı çıkarmıyor). Form türü başına ayrı entity (`ContactSubmission`, `SampleRequestSubmission` vb.) — değerlendirildi, reddedildi (admin'in "tek gelen kutusu" ihtiyacıyla çelişir, üç-tablo UNION sorgusu gerektirirdi). Public gönderim endpoint'ini şimdiden ekleyip `[AllowAnonymous]` işaretlemek — değerlendirildi, reddedildi (ADR-001/002/009'u ihlal eder).

**Avantajları:** Basit, tek tablo; admin paneli kolayca birleşik listeleme yapabiliyor; gelecekteki public site entegrasyonu için `CreateSubmissionAsync` zaten hazır ve test edilmiş; pagination deseni büyüyen veri setleri için projede ilk kez kanıtlandı, gelecekteki benzer modüller (log/audit, form/lead) doğrudan tekrar kullanabilir.

**Dezavantajları/Bilinen Riskler:** Form türleri gelecekte belirgin şekilde farklılaşırsa (örn. çok sayıda tip-özel alan) tek-tablo modeli nullable-sütun şişmesine yol açabilir — şu an 3 tip ve az sayıda tip-özel alanla bu risk düşük. E-posta bildirimi olmadan admin, yeni başvuruları yalnızca panele bakarak fark edebilir (public site fazına kadar kabul edilen bir sınırlama).

**Dokümandaki Dayanağı:** Madde 14.3 (İletişim/Bilgi Talep Akışı), Madde 17.2 (Form Yönetimi modülü), Madde 26 (Showroom randevu formu — ADR-008 ile zaten kapalı), Madde 29/29.1/29.2/29.3/29.4 (Formlar, form güvenliği), Madde 30 (RBAC), Madde 36.1 (Forms tablosu).

**Durum:** Onaylandı (19.07.2026, görev talimatındaki karar çerçevesi doğrultusunda, kritik bir çelişki oluşmadığı için onay beklenmeden uygulandı).

---

## ADR-016 — Identity'ye Bağımlı Servisler İçin Arayüz-Application/İmplementasyon-Infrastructure Deseni (Backlog #2, Task 16B)

**Karar:** `ApplicationUser`/`UserManager<ApplicationUser>`/`RoleManager<IdentityRole>` (Infrastructure.Identity) gibi Infrastructure katmanına özgü tiplere bağımlı olmak zorunda olan bir servis gerektiğinde, servis **arayüzü** (yalnızca primitive/DTO tipler kullanan) `Application` katmanında tanımlanır; **implementasyonu** `Infrastructure` katmanında (ilgili Infrastructure alt-namespace'inde, ör. `Infrastructure/Identity/`) yazılır ve Infrastructure'ın DI kayıt noktasından (`AddInfrastructureServices`) enjekte edilir. Bu, projedeki her Repository'nin zaten uyguladığı arayüz-Application/implementasyon-Infrastructure deseninin bir **servise** ilk uygulanışıdır (`DealerService`/`FormSubmissionService`/vb. gibi diğer servisler interface'siz concrete class kalmaya devam eder — bu genel bir stil değişikliği değildir).

**Güncelleme (20.07.2026, Task 17):** Desen ikinci kez uygulandı — `IRoleManagementService`/`RoleManagementService` (Role Management, salt-okunur) aynı gerekçeyle (RoleManager/UserManager'a bağımlılık) aynı deseni kullanıyor. Bu, ADR'nin "gelecekteki benzer ihtiyaçlar aynı deseni tekrar kullanabilir" öngörüsünü doğruladı — artık projede Identity'ye bağımlı **iki** interface'li servis var (`UserManagementService`, `RoleManagementService`), aşağıdaki "tek örnek" ifadesi bu genişlemeyle güncel değil, bkz. düzeltme notu.

**Güncelleme (20.07.2026, Task 18):** Desen **genişletilerek** üçüncü kez uygulandı — `IDashboardService`/`DashboardService` (Dashboard, salt-okunur) bu kez Identity tiplerine değil, doğrudan `AppDbContext`'e (Infrastructure.Persistence) bağımlı; sebep aynı: `Application` katmanı `Infrastructure`'a referans veremiyor. Bu, ADR'nin kapsamının aslında "Identity'ye özgü" değil, **herhangi bir Infrastructure-only tipe bağımlılık** olduğunu netleştirdi (Identity tipleri yalnızca ilk iki uygulamanın somut örneğiydi). Mevcut 6 modül repository'sine yeni `CountAsync` metodu eklemek yerine bu yol seçildi — Dashboard'ın 6 farklı entity'ye tek seferlik salt-okunur erişimi için repository arayüzlerini genişletmek scope creep sayıldı.

**Sebebi:** Mevcut proje reference graph'ı (`Presentation→(Application,Infrastructure)`; `Application→Domain`; `Infrastructure→(Application,Domain)`; `Domain→(yok)`) gereği `Application` projesi `Infrastructure`'a referans **veremez**. `UserManagementService` (Task 16B — Kullanıcı Yönetimi) `UserManager<ApplicationUser>`/`RoleManager<IdentityRole>`'a doğrudan bağımlı olmak zorundaydı (ADR-005: "Identity'nin altyapısı iş gereksinimi olarak sarmalanmayacak, sıfırdan RBAC yazılmayacak") — bu tipler yalnızca Infrastructure'da tanımlı. Servisin kendisini tamamen Infrastructure'a taşımak (arayüzsüz) `Presentation`'ın `Infrastructure.Identity`'ye doğrudan bağımlı olmasını gerektirirdi (AccountController zaten bunu yapıyor, ama genel prensip: modül servisleri her zaman Application katmanı arayüzü üzerinden tüketilir) — arayüz ayrımı bu tutarlılığı korur.

**Kesin kapsam:**
- `Application/Users/IUserManagementService.cs` + `UserDto`/`CreateUserRequest`/`UpdateUserRequest`/`ResetUserPasswordRequest`/`UserOperationResult` — hiçbiri `ApplicationUser` veya başka bir Infrastructure tipine referans vermez.
- `Infrastructure/Identity/UserManagementService.cs` — `IUserManagementService`'i implemente eder, `UserManager`/`RoleManager` kullanır, `Infrastructure/DependencyInjection.cs`'te kayıtlıdır.
- `UserController` (Presentation), yalnızca `IUserManagementService`'e bağımlıdır — diğer tüm controller'ların kendi Application servisine bağımlı olma deseniyle tutarlı, `Infrastructure.Identity`'ye (AccountController hariç) hiç dokunmaz.

**Alternatifleri:** Servisi tamamen Infrastructure'da concrete class olarak bırakmak, Controller'ın doğrudan `Infrastructure.Identity.UserManagementService`'e bağımlı olması (değerlendirildi, reddedildi — Presentation'ın modül servislerini her zaman Application arayüzü/sınıfı üzerinden tükettiği genel deseni bozar). `Application`'a `Infrastructure`'a referans izni vermek (değerlendirildi, reddedildi — döngüsel bağımlılık, .NET proje sistemi zaten izin vermez, Infrastructure zaten Application'a bağımlı).

**Avantajları:** Katman sınırı korunur; `UserController` test edilirken yalnızca `IUserManagementService` mock'lanması/sahtelenmesi yeterli olur (gerekirse); gelecekteki benzer ihtiyaçlar (ör. Role Management servisi, eğer/ne zaman geliştirilirse) aynı deseni tekrar kullanabilir.

**Dezavantajları:** Projede artık iki farklı servis kayıt deseni var (concrete-class-only vs interface+implementation) — ama bu, gerçek bir mimari zorunluluktan kaynaklanıyor, keyfi bir tutarsızlık değil; bu ADR'nin amacı da bunu gelecekteki geliştiriciler için açıkça gerekçelendirmek.

**Dokümandaki Dayanağı:** ADR-005 (Identity altyapısının sarmalanmaması ilkesi), mevcut proje reference graph'ı (Task 1.1'den beri değişmedi).

**Durum:** Onaylandı (20.07.2026, Task 16B implementasyonu sırasında, kritik bir çelişki oluşturmadığı için onay beklenmeden uygulandı).

---

## Henüz Alınmamış Kararlar (Bilinçli Olarak Açık Bırakıldı)

Aşağıdaki konular **kesin mimari karar olarak kaydedilmemiştir**; Task 0.2 veya ilgili modül task'ında değerlendirilecektir. Bu listeye "karar" statüsü verilmesi yasaktır:

- ~~EF Core Code First vs Database First~~ — **Çözüldü, bkz. ADR-004 (Code First, Onaylandı 17.07.2026).**
- ~~Panel authentication yöntemi: Cookie/ASP.NET Identity vs JWT~~ — **Çözüldü, bkz. ADR-005 (Identity + Cookie, Onaylandı 17.07.2026).**
- **(Yeni, ADR-005'ten)** 2FA ekranları, aktivasyon süreci, recovery code yönetimi ve zorunluluk politikası — gelecekte ayrı bir task.
- **(Yeni, ADR-005'ten)** Şifre sıfırlama akışının bu fazda uygulanıp uygulanmayacağı + e-posta sağlayıcısı/mail gönderim altyapısı seçimi — açık karar.
- ~~Dosya saklama yöntemi: dosya sistemi vs blob storage~~ — **Çözüldü, bkz. ADR-006 (Yerel Dosya Sistemi + storage abstraction, Onaylandı 17.07.2026).**
- **(Yeni, ADR-006'dan)** Dosya isimlendirme standardı (Madde 37.4 — ayrıca netleştirilecek).
- **(Yeni, ADR-006'dan)** WebP/AVIF dönüşümü, responsive image üretimi ve CDN kullanımı — ayrı alt karar/task.
- **(Yeni, ADR-006'dan, risk olarak kayıtlı)** Çoklu instance deployment senaryosunda yerel dosya sisteminin yetersiz kalma riski — deployment kararına bağlı, henüz aksiyon gerektirmiyor.
- Çoklu dil fallback davranışı (Madde 28.3 — "Karar Bekleniyor"). **Durum: Gelecek Faz / Karar Bekleniyor (ADR-007 ile teyit edildi — public site kapsamına ait, ADR-002 gereği bu fazın dışında).**
- ~~Translations tablosunun nihai şeması: `FieldName` yapısı, `Value` tipi/uzunluğu, alan-bazlı/tek-kayıt modeli, `EntityType` temsili, unique index/constraint detayları~~ — **Çözüldü, bkz. ADR-012 (Onaylandı 19.07.2026).**
- ~~Polimorfik `EntityId` ilişkisi nedeniyle klasik FK kısıtlarının sınırlı olması ve yetim (orphan) çeviri kayıtlarının temizlenme stratejisi~~ — **Çözüldü, bkz. ADR-012 (Application katmanı, aynı transaction, trigger/job yok — Onaylandı 19.07.2026).** Polimorfik yapının kendisinden kaynaklanan DB-seviyesi FK eksikliği kalıcı bir yapısal risk olarak ADR-012'nin "Dezavantajları" bölümünde kayıtlıdır.
- **(Yeni, ADR-012'den)** `Language` entity'sinin ayrıntılı alan listesi (Task 3.1B'de netleşecek) ve `Translation`/`Language` migration'ının uygulanması — henüz yapılmadı, Task 3.1B'nin konusu.
- ~~Showroom'un ayrı modül mü, Bayi kategorisi mi olacağı~~ — **Çözüldü, bkz. ADR-008 (Tek Dealer entity + Category ayrımı, Onaylandı 17.07.2026).**
- ~~Kategorisiz (17) Bayi/Showroom kaydının nasıl ele alınacağı~~ — **Çözüldü, bkz. ADR-008 güncellemesi (Task 14, 19.07.2026) — nullable `Category` alanı.**
- ~~Showroom-özel alanların (galeri görselleri, çalışma saatleri, randevu talep formu) bu fazda uygulanıp uygulanmayacağı~~ — **Çözüldü, bkz. ADR-008 güncellemesi (Task 14, 19.07.2026) — uygulanmadı, Madde 25.1'in gerçek veri modeli tablosunda yoklar, yalnızca public-site anlatımında (Madde 26) geçiyorlar.**
- ~~Güvenlik gereksinimlerinin teknik uygulama yöntemleri (rate limiting, loglama, dosya doğrulama)~~ — **Çözüldü, bkz. ADR-011 (yerleşik Rate Limiting + Serilog/ILogger + MIME/magic-byte/whitelist, Onaylandı 17.07.2026).**
- **(Yeni, ADR-011'den)** Rate limiting'in gerçek limit değerleri — ilgili güvenlik task'ında.
- **(Yeni, ADR-011'den)** Log hedefleri (dosya/SQL/Seq vb.) — deployment kararında.
- Yedekleme stratejisi — bir kod/mimari kararı değil, hosting/operasyon kararı (deployment fazına bırakıldı).
- ~~SAP API (Madde 39) endpoint'lerinin Faz 1/Faz 2 sınırı~~ — **Çözüldü, bkz. ADR-010 (Tamamen Faz 2, Faz 1'de hiçbir hazırlık yok, Onaylandı 17.07.2026).**

---

*Faz 0 — Task 0.1 kapsamında henüz teknik bir mimari karar alınmamıştır (kod yazma yasağı nedeniyle). Tek karar ADR-001'deki kapsam kararıdır.*
