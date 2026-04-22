using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Interfaces;
using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using RecipeShareData;
using System.Security.Claims;

namespace RecipeShare_WebAPP.Controllers
{
    [Authorize] 
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly RecipeShareContext _context;

        
        public RecipeController(IRecipeService recipeService, RecipeShareContext context)
        {
            _recipeService = recipeService;
            _context = context;
        }


        [AllowAnonymous] 
        public async Task<IActionResult> Index(string? category, string? difficulty, int? maxTime)
        {
            var model = await _recipeService.GetAllAsync(category, difficulty, maxTime);
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var model = await _recipeService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _recipeService.GetCreateModelAsync();
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(RecipeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var createModel = await _recipeService.GetCreateModelAsync();
                model.Categories = createModel.Categories;
                model.Difficulties = createModel.Difficulties;

                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _recipeService.CreateAsync(model, userId!);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _recipeService.GetEditModelAsync(id, userId!);


            return View("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecipeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var editModel = await _recipeService.GetEditModelAsync(model.Id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (editModel != null)
                {
                    model.Categories = editModel.Categories;
                    model.Difficulties = editModel.Difficulties;
                }
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _recipeService.UpdateAsync(model, userId!);

            if (!result) return Unauthorized();

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _recipeService.GetDeleteModelAsync(id, userId!);

            if (model == null) return Unauthorized();

            return View(model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _recipeService.DeleteAsync(id, userId!);

            if (!result) return Unauthorized();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> SearchByComponent()
        {
            var components = await _context.Components.OrderBy(c => c.Name).ToListAsync();
            return View(components);
        }


        [HttpPost]
        public async Task<IActionResult> SearchByComponent(Guid componentId)
        {
            var recipes = await _recipeService.GetRecipesByComponentAsync(componentId);

            var model = new RecipeIndexViewModel
            {
                Recipes = recipes,
                Categories = await _context.Categories.Select(c => c.Name).ToListAsync(),
                Difficulties = new List<string> { "Ниска", "Средна", "Висока" }
            };

            return View("Index", model); 
        }
    }
}