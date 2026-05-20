using System.Security.Claims;

namespace ItBudgetingV4.Web.Services;

public interface ICurrentUserService
{
    string GetUsername(ClaimsPrincipal user);
}

public class CurrentUserService : ICurrentUserService
{
    public string GetUsername(ClaimsPrincipal user) => user.Identity?.Name ?? "system";
}
