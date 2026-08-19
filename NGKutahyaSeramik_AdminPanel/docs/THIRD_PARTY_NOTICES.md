# Third-Party Notices

Bu proje (NG Kütahya Seramik Admin Panel — Presentation/UI katmanı), aşağıdaki açık kaynak
bileşenleri **yerel olarak** (`wwwroot/lib/`, CDN kullanılmadan) barındırır. Her ikisi de MIT
lisanslıdır; lisans metinleri aşağıda tam olarak yer almaktadır.

---

## Tabler (Core)

- **Sürüm:** 1.4.0 (sabitlenmiş — `latest` veya değişken sürüm kullanılmadı)
- **Kaynak:** https://github.com/tabler/tabler
- **npm paketi:** `@tabler/core@1.4.0`
- **Kullanılan dosyalar:** `wwwroot/lib/tabler/css/tabler.min.css`, `wwwroot/lib/tabler/js/tabler.min.js`
- **Lisans:** MIT

```
The MIT License (MIT)

Copyright (c) 2018-2026 The Tabler Authors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

---

## Tabler Icons (Web Font)

- **Sürüm:** 3.45.0 (sabitlenmiş)
- **Kaynak:** https://github.com/tabler/tabler-icons (packages/icons-webfont)
- **npm paketi:** `@tabler/icons-webfont@3.45.0`
- **Kullanılan dosyalar:** `wwwroot/lib/tabler-icons/tabler-icons.min.css`,
  `wwwroot/lib/tabler-icons/fonts/tabler-icons.{woff2,woff,ttf}`
- **Lisans:** MIT

```
MIT License

Copyright (c) 2020-2026 Paweł Kuna

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## ClosedXML (Excel Import — backlog #17)

- **Sürüm:** 0.104.2 (NuGet, `dotnet add package` ile eklendi, sabit sürüm)
- **Kaynak:** https://github.com/ClosedXML/ClosedXML
- **Kullanım amacı:** `.xlsx` şablon üretimi (indirme) ve yüklenen `.xlsx` dosyalarının okunması — yalnızca `Infrastructure/ProductImport/` içinde; `Application` katmanı bu kütüphaneye hiç bağımlı değil, yalnızca kendi `IProductImportFileReader`/`IProductImportTemplateWriter` arayüzlerine (storage abstraction ilkesiyle tutarlı).
- **Lisans:** MIT

ClosedXML ile birlikte gelen ve NuGet üzerinden otomatik kurulan bağımlılıklar (hepsi lisans metadata'sından doğrulanmıştır):

| Paket | Sürüm | Lisans |
|---|---|---|
| ClosedXML | 0.104.2 | MIT |
| ClosedXML.Parser | 1.2.0 | MIT |
| DocumentFormat.OpenXml | 3.1.1 | MIT |
| DocumentFormat.OpenXml.Framework | 3.1.1 | MIT |
| ExcelNumberFormat | 1.1.0 | MIT |
| RBush | 4.0.0 | MIT |
| System.IO.Packaging | 8.0.1 | MIT |
| SixLabors.Fonts | 1.0.0 | Apache-2.0 |

```
The MIT License (MIT)

Copyright (c) ClosedXML and contributors (ve yukarıdaki MIT lisanslı bağımlılıkların ilgili sahipleri)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

`SixLabors.Fonts` (1.0.0) — Apache License 2.0 kapsamındadır (yazı tipi ölçümü için ClosedXML'in dolaylı bağımlılığı, bu projede doğrudan kullanılmaz). Tam lisans metni: https://www.apache.org/licenses/LICENSE-2.0 — özet: kaynak kodun/telif bildiriminin korunması şartıyla kullanım, değişiklik, dağıtım serbesttir; NOTICE dosyası değişikliği yoktur (Six Labors bu sürüm için ek NOTICE dosyası yayımlamamıştır).

---

## Notlar

- Her iki paket de npm registry üzerinden indirilip yalnızca gerekli derlenmiş (compiled) dosyalar
  projeye kopyalanmıştır — `pnpm` build sistemi, Liquid template sistemi, Tabler demo backend'i,
  demo veri, Tabler'ın önizleme uygulaması ve React/Vue/Angular sürümleri projeye dahil edilmemiştir.
- Tüm dosyalar `wwwroot/lib/` altında yerel olarak barındırılıyor — hiçbir CDN'e bağımlılık yoktur.
- `tabler.min.css` incelendiğinde hiçbir harici `@import` veya Google Fonts referansı bulunmadığı
  doğrulanmıştır (tüm ikonografik SVG'ler data-URI olarak gömülüdür).
- ClosedXML ve bağımlılıkları standart NuGet paket referansı (`PackageReference`) ile eklenmiştir —
  kaynak kodu projeye kopyalanmamıştır, `dotnet restore` ile NuGet.org'dan çözümlenir.
