using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class DealerFactory
{
    public static Dealer CreateDealer(string name = "Test Bayi", string city = "İstanbul") =>
        new(name, city, DealerCategory.SalesPoint, "Kadıköy", "Test Adres", "05551234567", null, "test@example.com", null,
            40.990000m, 29.030000m, "SADN", "Anadolu Yakası");

    public static Dealer CreateShowroom(string name = "Test Showroom", string city = "Kütahya") =>
        new(name, city, DealerCategory.Factory, null, null, null, null, null, null, null, null, null, null);

    public static Dealer CreateUnclassified(string name = "Kategorisiz Kayıt", string city = "Bursa") =>
        new(name, city, null, null, null, null, null, null, null, null, null, null, null);
}
