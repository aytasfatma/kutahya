namespace Presentation.Models.AboutManagement;

public sealed class AboutManagementViewModel
{
    public string HeaderEyebrow { get; set; } = "Kurumsal";
    public string HeaderTitle { get; set; } = "Yarım Asırlık Zanaatten\nGeleceğin Yüzeylerine";
    public string HeaderDescription { get; set; } = "NG Kütahya Seramik çatısı altında NG Kütahya Seramik, NG Stone, NG Slim ve NG Performa markalarıyla; mimari vizyonu güçlü, sürdürülebilir ve estetik yüzeyler üretiyoruz. Kütahya'daki köklerimizden aldığımız zanaat bilgisini modern üretim teknolojisiyle buluşturuyoruz.";
    public string VisionTitle { get; set; } = "Vizyonumuz";
    public string VisionSubtitle { get; set; } = "Geleceğe Yön Veren Değerlerimiz";
    public string VisionText { get; set; } = "Mimari yüzeylerde yenilikçi, sürdürülebilir ve ilham veren çözümlerle dünyada tercih edilen bir marka olmak.";
    public string MissionTitle { get; set; } = "Misyonumuz";
    public string MissionSubtitle { get; set; } = "Sürdürülebilir Değer Üretme Yaklaşımımız";
    public string MissionText { get; set; } = "Zanaat bilgisini teknolojiyle buluşturarak estetik, dayanıklı ve sorumlu yüzeyler üretmek.";
    public List<AboutStatisticItem> StatisticItems { get; set; } =
    [
        new() { Value = "1.170+", Label = "Aktif Ürün" },
        new() { Value = "272", Label = "Koleksiyon" },
        new() { Value = "4", Label = "Marka" },
        new() { Value = "212", Label = "Satış Noktası" },
        new() { Value = "63", Label = "Şehir" },
        new() { Value = "7", Label = "Dilde Hizmet" }
    ];
    public string HistoryTitle { get; set; } = "Zanaatten Endüstriyel Ölçeğe Bir Yolculuk";
    public string HistoryDescription { get; set; } = "Kütahya'nın köklü seramik geleneğinden güç alan yolculuğumuz, bugün dört markalı geniş bir ürün ailesine uzanıyor.";
    public string HistoryItems { get; set; } = "Kuruluş|Kütahya'da İlk Adım|Bölgenin köklü seramik ve porselen üretim birikiminden ilham alarak üretime başladık.\nBüyüme Dönemi|Üretim Kapasitesinin Genişlemesi|Artan talebe paralel olarak üretim tesislerimizi büyüttük, ürün gamımızı çeşitlendirdik.\nMarka Genişlemesi|NG Stone, NG Slim ve NG Performa|Doğal taş görünümlü, ince kesit ve teknik performans odaklı yeni markalarımızı hayata geçirdik.\nUluslararasılaşma|Bayi ve Showroom Ağının Gelişmesi|Satış noktası ağımızı yurt içinde ve yurt dışında genişlettik.\nBugün|Dijital Deneyim Dönemi|Mimari vizyonu güçlü bir dijital deneyim inşa ediyoruz.";
    public string ValuesTitle { get; set; } = "Bizi Biz Yapan İlkeler";
    public string ValuesDescription { get; set; } = "Her koleksiyonun ve her üretim adımının arkasında duran değerler.";
    public string Values { get; set; } = "Kalite|Her üretim aşamasında titiz kalite kontrolü ile tutarlı ve uzun ömürlü yüzeyler sunuyoruz.\nSürdürülebilirlik|Enerji, su ve kaynak yönetiminde sorumlu üretim yaklaşımını benimsiyoruz.\nİnovasyon|Yüzey teknolojilerini ve üretim süreçlerini sürekli geliştiriyoruz.\nEstetik|Mimari vizyonu destekleyen, doğadan ilham alan tasarım dilini önceliklendiriyoruz.\nGüvenilirlik|Bayi ve iş ortaklarımızla şeffaf, uzun soluklu ilişkiler kuruyoruz.\nMüşteri Odaklılık|Her kullanıcı grubunun ihtiyacını merkeze alıyoruz.\nZanaat Bilgisi|Kütahya'nın köklü seramik geleneğini modern üretim teknolojisiyle harmanlıyoruz.\nŞeffaflık|Teknik verilerimizi ve sertifikalarımızı açık ve erişilebilir şekilde paylaşıyoruz.";
    public string ProductionTitle { get; set; } = "Modern Teknoloji, Geniş Ürün Yelpazesi";
    public string ProductionText { get; set; } = "Sırlı porselen, teknik granit seramik ve duvar karosu bünyelerinde; 28 farklı ebat ve 18 farklı yüzey seçeneğiyle geniş bir üretim kapasitesine sahibiz.";
    public string ProductionItems { get; set; } = "28 Ebat|60×120'den 120×280'e uzanan geniş ebat seçenekleri.\n18 Yüzey Tipi|Mat, parlak, nano, lappato ve satinato dahil yüzey işlemleri.\n3 Bünye Tipi|Sırlı porselen, teknik granit seramik ve duvar karosu.";
    public string AwardsTitle { get; set; } = "Sektörde Tanınırlığımız";
    public string AwardsDescription { get; set; } = "Tasarım, üretim kalitesi ve sürdürülebilirlik alanındaki çalışmalarımız.";
    public string Awards { get; set; } = "Tasarım Mükemmelliği Kategorisi|İçerik Bekleniyor\nKalite Yönetiminde Sektörel Tanınırlık|İçerik Bekleniyor\nSürdürülebilir Üretim Girişimleri|İçerik Bekleniyor\nİhracat Performansı Değerlendirmeleri|İçerik Bekleniyor";
    public string CertificatesTitle { get; set; } = "Kalite ve Uygunluk Belgelerimiz";
    public string CertificatesDescription { get; set; } = "Üretim süreçlerimiz uluslararası kalite ve çevre yönetim standartlarına uygun şekilde yürütülür.";
    public string PartnershipsTitle { get; set; } = "Birlikte Büyüdüğümüz Paydaşlar";
    public string PartnershipsDescription { get; set; } = "Ürünlerimizi doğru projelerle buluşturan geniş bir iş birliği ağına sahibiz.";
    public string Partnerships { get; set; } = "Mimarlık & İç Mimarlık Ofisleri|Proje bazlı ürün seçimi ve teknik danışmanlık süreçlerinde birlikte çalıştığımız tasarım ofisleri.\nBayi ve Showroom Ağı|Ürünlerimizi son kullanıcıya ulaştıran kanal ortaklarımız.\nSektörel Fuar ve Etkinlikler|Markalarımızı ve koleksiyonlarımızı sektörle buluşturuyoruz.";
    public string InformationTitle { get; set; } = "6102 Sayılı Türk Ticaret Kanunu Kapsamında Bilgi Toplumu Hizmetleri";
    public string InformationDescription { get; set; } = "Türk Ticaret Kanunu'nun 1524. maddesi uyarınca internet sitesi üzerinden erişime açık tutulması gereken şirket bilgileri bu bölümde yayınlanır.";
}

public sealed class AboutStatisticItem
{
    public string Value { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? IconPath { get; set; }
    public bool RemoveIcon { get; set; }
    public bool Hidden { get; set; }
}
