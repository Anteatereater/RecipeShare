using Microsoft.AspNetCore.Mvc;
using RecipeShareData;

namespace RecipeShare.WebApp.Controllers
{
    public class RecipeShareController : Controller
    {
        private readonly RecipeShareContext context;

        public RecipeShareController(RecipeShareContext context)
        {
            this.context = context;
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
