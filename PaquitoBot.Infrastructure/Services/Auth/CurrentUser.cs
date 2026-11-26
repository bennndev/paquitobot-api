using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAdministrator => httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;

    public Guid? StudentId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("student_id");
            return Guid.TryParse(value, out var studentId) ? studentId : null;
        }
    }

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
