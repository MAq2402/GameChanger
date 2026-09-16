using System.Security.Claims;

namespace GameChanger.Api.Security;

public interface ICurrentOwner
{
    string? OwnerId { get; }
}

public sealed class CurrentOwner(
    IHttpContextAccessor httpContextAccessor,
    IHostEnvironment environment) : ICurrentOwner
{
    public string? OwnerId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var authenticatedOwnerId = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(authenticatedOwnerId))
            {
                return authenticatedOwnerId;
            }

            return environment.IsDevelopment() || environment.IsEnvironment("Testing")
                ? "local-development-owner"
                : null;
        }
    }
}
