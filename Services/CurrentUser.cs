using System.Security.Claims;
using SupplyChain.Application.Abstractions;
using SupplyChain.Application.Authorization;

namespace SupplyChain.Web.Services;

/// <inheritdoc cref="ICurrentUser" />
/// <remarks>
/// Reads from the signed authentication cookie rather than from mutable server state. The legacy
/// application kept the current user in a global variable that any form could overwrite.
/// </remarks>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>Initialises a new instance.</summary>
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    /// <inheritdoc />
    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public string? UserName => Principal?.Identity?.Name;

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    /// <remarks>
    /// Successor to the legacy <c>PCName</c> audit column, which was populated from
    /// <c>fncComputerName()</c>. Over HTTP the nearest equivalent is the caller's address.
    /// </remarks>
    public string? ClientIdentifier => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    /// <inheritdoc />
    public bool HasPermission(string permission) =>
        Principal?.HasClaim(Permissions.ClaimType, permission) ?? false;
}
