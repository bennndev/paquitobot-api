using System.Security.Claims;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Api.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var subject = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");

            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
    }
}
