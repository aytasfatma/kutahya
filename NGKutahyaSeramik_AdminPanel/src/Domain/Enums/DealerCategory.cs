namespace Domain.Enums;

/// <summary>Madde 25.1 — "Category enum, Bayi (2), Showroom (3)." Mevcut kaynak veriyle (Ek-2) uyumlu
/// olması için alttaki sayısal değerler doküman/legacy veriyle birebir eşleştirildi (0/1 değil).</summary>
public enum DealerCategory
{
    GeneralHeadquarters = 1,
    Factory = 2,
    SalesPoint = 3
}
