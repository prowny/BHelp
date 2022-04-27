using System.Linq;
using System.Security.Principal;

namespace BHelp
{
    public static class PriincipalExtensions
    {
        public static bool IsInAnyRoles(this IPrincipal principal, params string[] roles)
        {
            return roles.Any(r => principal.IsInRole(r));
        }
    }
}