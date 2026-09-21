using System.Reflection;
using System.Security.Claims;

namespace Ping.Api.Authentication;

public sealed class CurrentUser : IBindableFromHttpContext<CurrentUser>
{
    private CurrentUser(ClaimsPrincipal principal)
    {
        Principal = principal;
        IsAuthenticated = principal.Identity?.IsAuthenticated ?? false;

        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(rawId, out var id) && id != 0)
            Id = id;

        Username = principal.FindFirstValue(ClaimTypes.Name);
    }

    public ClaimsPrincipal Principal { get; }

    public bool IsAuthenticated { get; }

    public long? Id { get; }

    public string? Username { get; }

    public long RequiredId => Id ?? throw new InvalidOperationException("User Id claim is missing or invalid.");

    public static ValueTask<CurrentUser?> BindAsync(HttpContext context, ParameterInfo parameter) =>
        ValueTask.FromResult<CurrentUser?>(new CurrentUser(context.User));
}
