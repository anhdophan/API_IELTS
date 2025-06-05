using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace api.Pages.Admin
{
    public class AdminPageBase : PageModel
    {
        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var isLoggedIn = HttpContext.Session.GetString("IsAdminLoggedIn");
            var currentPage = context.ActionDescriptor.ViewEnginePath.ToLower();

            // Allow access to login page without redirect
            if (currentPage.Contains("login_admin"))
                return;

            if (isLoggedIn != "true")
            {
                context.Result = new RedirectToPageResult("/Admin/Login_Admin");
            }
        }
    }
}