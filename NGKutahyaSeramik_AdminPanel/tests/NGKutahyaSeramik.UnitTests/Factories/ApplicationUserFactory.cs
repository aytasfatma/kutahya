using Infrastructure.Identity;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class ApplicationUserFactory
{
    public static ApplicationUser CreateAdmin(string email = "admin@test.local") => Create(email);

    public static ApplicationUser CreateContentEditor(string email = "editor@test.local") => Create(email);

    public static ApplicationUser CreateSeoEditor(string email = "seo@test.local") => Create(email);

    public static ApplicationUser CreateProductManager(string email = "product@test.local") => Create(email);

    public static ApplicationUser CreateWithoutRole(string email = "norole@test.local") => Create(email);

    private static ApplicationUser Create(string email) => new()
    {
        UserName = email,
        Email = email,
        EmailConfirmed = true
    };
}
