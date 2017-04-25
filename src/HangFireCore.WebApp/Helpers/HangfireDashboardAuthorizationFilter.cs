using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace HangFireCore.WebApp.Helpers
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpcontext = context.GetHttpContext();

            return httpcontext.User.Identity.IsAuthenticated;
        }
    }
}
