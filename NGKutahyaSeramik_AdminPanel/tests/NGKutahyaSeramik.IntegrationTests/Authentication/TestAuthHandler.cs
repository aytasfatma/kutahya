using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NGKutahyaSeramik.IntegrationTests.Authentication;

/// <summary>
/// Gerçek cookie/Identity oturum akışı yerine, `X-Test-Role` header'ından okunan role/rollere göre
/// bir ClaimsPrincipal üreten test-only authentication handler'ı. Header yoksa istek anonim kalır
/// (AuthenticateResult.NoResult) — [Authorize] her zamanki gibi 401/403 ile tepki verir; gerçek
/// login/cookie akışı test edilmiyor, yalnızca RBAC/[Authorize(Roles=...)] doğrulanıyor.
/// AntiForgery bu handler'dan tamamen bağımsız çalışmaya devam eder — güvenlik kontrolü kapatılmaz.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string RoleHeaderName = "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RoleHeaderName, out var roleHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var roles = roleHeaderValues
            .SelectMany(v => (v ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToArray();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Name, "test-user")
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
