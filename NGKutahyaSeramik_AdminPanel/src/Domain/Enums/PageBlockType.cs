namespace Domain.Enums;

/// <summary>
/// Madde 16.2 — "İçerik blokları esnek yapıda olacak: metin + görsel, tam genişlik görsel,
/// video embed, akordeon, tab yapısı." 5 blok tipi dokümanda birebir sayılmış. Accordion/Tab'ın
/// çoklu panel/sekme iç yapısı dokümanda tanımlanmadığı için her blok bağımsız bir içerik birimidir
/// (bkz. PageContentBlock).
/// </summary>
public enum PageBlockType
{
    TextImage,
    FullWidthImage,
    VideoEmbed,
    Accordion,
    Tab
}
