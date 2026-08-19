using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

/// <summary>
/// Geçerli varsayılan değerlerle bir Product örneği üretir; testin ilgilendiği alanlar override
/// edilebilir. ProductService business logic'i burada kopyalanmaz — yalnızca entity invariant'larını
/// (zorunlu native alanlar) karşılayan bir kurulum sağlar.
/// </summary>
public static class ProductFactory
{
    public static Product CreateValid(
        string productCode = "TEST0001RP",
        int categoryId = 1,
        int collectionId = 1,
        ProductBrand brand = ProductBrand.NgSeramik,
        ProductStatus status = ProductStatus.Active,
        string size = "60x120",
        string unit = "M2",
        string surface = "MAT",
        decimal thickness = 9.0m,
        string bodyType = "SIRLI PORSELEN",
        string color = "Beyaz",
        string applicationArea = "YER",
        string usageArea = "BANYO",
        int displayOrder = 0) =>
        new(
            productCode: productCode,
            categoryId: categoryId,
            collectionId: collectionId,
            brand: brand,
            status: status,
            commercialName: null,
            productGroup: null,
            size: size,
            unit: unit,
            surface: surface,
            relief: null, specialSurface: null, faceCount: null,
            thickness: thickness, bodyType: bodyType, color: color, colorMaterial: null,
            applicationArea: applicationArea, usageArea: usageArea, finish: null, pei: null,
            vValue: null, rValue: null, deepAbrasion: null,
            heatResistance: null, antiSlip: null, glazedGranite: null,
            hasFace: null, hasVenue: null,
            boxM2: null, palletM2: null, displayOrder);
}
