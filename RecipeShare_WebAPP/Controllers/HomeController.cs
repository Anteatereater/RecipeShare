using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeShare_WebAPP.Models;
using RecipeShare_WebAPP.Models.Home;
using RecipeShareData;
using System.Diagnostics;

namespace RecipeShare_WebAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly RecipeShareContext _context;

        public HomeController(RecipeShareContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recipes = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.Images)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(6)
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                LatestRecipes = recipes
            };

            return View(model);
        }
    }
}
