using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace api.Pages.Admin
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            ErrorMessage = "";
        }

        public IActionResult OnPost()
        {
            if (Username == "Admin123" && Password == "Admin123")
            {
                HttpContext.Session.SetString("IsAdminLoggedIn", "true");
                return RedirectToPage("/Admin/Courses/Index");
            }
            else
            {
                ErrorMessage = "No Permission";
                return Page();
            }
        }
    }
}