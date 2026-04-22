using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RecipeShareData;
using RecipeShareData.Entities;

namespace RecipeShare_WebAPP.Controllers
{
    [Authorize]
    public class ComponentsController : Controller
    {
        private readonly RecipeShareContext _context;

        public ComponentsController(RecipeShareContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Component model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Components.Add(model);
            await _context.SaveChangesAsync();


            return RedirectToAction("Create");
        }
    }
}