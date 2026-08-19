using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class FormSubmissionFactory
{
    public static FormSubmission CreateContact(string fullName = "Test Kullanıcı", string email = "test@example.com") =>
        new(FormType.Contact, fullName, email, "05551234567", "Test Firma", "Test mesajı", true,
            subject: "Genel Bilgi", productCode: null, productName: null, address: null, requestedProduct: null, quantity: null);

    public static FormSubmission CreateRequestInformation(string productCode = "55018167RP") =>
        new(FormType.RequestInformation, "Test Kullanıcı", "test@example.com", "05551234567", null, "Bilgi almak istiyorum", true,
            subject: null, productCode: productCode, productName: "AMAZONIT", address: null, requestedProduct: null, quantity: null);

    public static FormSubmission CreateSampleRequest(int quantity = 2) =>
        new(FormType.SampleRequest, "Test Mimar", "mimar@example.com", "05551234567", "Mimar Ofisi", "Numune talep ediyorum", true,
            subject: null, productCode: null, productName: null, address: "Test Adres, İstanbul",
            requestedProduct: "Amazonit", quantity: quantity);
}
