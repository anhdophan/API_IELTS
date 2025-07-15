using Microsoft.AspNetCore.Mvc.RazorPages;

namespace api.Pages.Admin
{
    public class ChatModel : PageModel
    {
        public void OnGet()
        {
            // Không cần xử lý backend vì dữ liệu sẽ được fetch từ client-side bằng JS
        }
    }
}
