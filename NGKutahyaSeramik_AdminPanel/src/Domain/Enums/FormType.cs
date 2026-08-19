namespace Domain.Enums;

/// <summary>Madde 29 — Formlar. Dokümanda somut alan listesiyle tanımlanan yalnızca 3 form türü var:
/// 29.1 İletişim Formu, 29.2 Request Information/Bilgi Talep Formu, 29.3 Numune Talep Formu.
/// Randevu talep formu (Madde 26, showroom'a özel, ADR-008'de "eklenebilir/Karar Bekleniyor" olarak
/// işaretli) ve bayi başvuru/kariyer formları dokümanda somut alan listesiyle tanımlanmadığı için
/// buraya eklenmedi.</summary>
public enum FormType
{
    Contact,
    RequestInformation,
    SampleRequest
}
