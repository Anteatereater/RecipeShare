using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Interfaces;
using RecipeShare.Web.ViewModels.Component;
using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using RecipeShareData;
using RecipeShareData.Entities;
using System.Security.Claims;
using static RecipeShare.Web.ViewModels.ViewModels.Recipes.RecipeDetailsViewModel;


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
        public async Task<IActionResult> Index(string? category, string? difficulty, int? maxTime, string? sortOrder)
        {
           
            var model = await _recipeService.GetAllAsync(category, difficulty, maxTime, sortOrder);
            return View(model);
        }




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
        public async Task<IActionResult> ByComponent(Guid id)
        {
            
            var component = await _context.Components.FindAsync(id);
            if (component == null) return RedirectToAction(nameof(Index));

           
            var viewModel = new RecipeIndexViewModel
            {
                Recipes = await _context.Recipes
                    .Where(r => r.ComponentRecipes.Any(cr => cr.ComponentId == id))
                    .Select(r => new RecipeListItemViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        ImageUrl = r.Images.Select(i => i.Url).FirstOrDefault(),
                        CategoryName = r.Category.Name,
                        Difficulty = r.Difficulty.ToString(),
                        AuthorName = r.User.UserName ?? "Анонимен",                    
                        Components = r.ComponentRecipes.Select(cr => new RecipeComponentViewModel
                        {
                            Id = cr.Component.Id,
                            Name = cr.Component.Name
                        }).ToList()
                    })
                    .ToListAsync()
            };

            ViewData["SearchTerm"] = component.Name;
            return View("SearchResults", viewModel); 
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

                var freshModel = await _recipeService.GetCreateModelAsync();
                model.AvailableComponents = freshModel.AvailableComponents;
                model.Categories = freshModel.Categories;
                model.Difficulties = freshModel.Difficulties;
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var recipe = new Recipe
            {
                Name = model.Name,
                Description = model.Description,
                PreparationTimeMinutes = model.PreparationTimeMinutes,
                Difficulty = Enum.Parse<DifficultyLevel>(model.Difficulty, true),
                CategoryId = model.CategoryId,
                UserId = userId!,
                CreatedAt = DateTime.UtcNow
            };

         
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                recipe.Images.Add(new Image { Url = model.ImageUrl, Name = model.ImageName ?? model.Name });
            }


            foreach (var item in model.SelectedComponents)
            {
                recipe.ComponentRecipes.Add(new ComponentRecipe { ComponentId = item.ComponentId });
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var recipe = await _context.Recipes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound("Рецептата не е намерена.");
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = recipe.UserId == currentUserId;

            if (!isAdmin && !isOwner)
            {
                return Forbid(); 
            }

            var model = await _recipeService.GetEditModelAsync(id, currentUserId!);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RecipeEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {

                var categoriesAndDifficulties = await _recipeService.GetCreateModelAsync(); 
                model.Categories = categoriesAndDifficulties.Categories;
                model.Difficulties = categoriesAndDifficulties.Difficulties;
             
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");


            bool isSaved = await _recipeService.EditAsync(id, model, userId!, isAdmin);

            if (!isSaved)
            {
                return Unauthorized("Нямате права да редактирате тази рецепта.");
            }


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
        [Authorize] 
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = recipe.UserId == currentUserId;

            
            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

           
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

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
            var component = await _context.Components.FindAsync(componentId);
            if (component == null) return RedirectToAction(nameof(Index));

            var viewModel = new RecipeIndexViewModel
            {
                Recipes = await _context.Recipes
                    .Where(r => r.ComponentRecipes.Any(cr => cr.ComponentId == componentId))
                    .Select(r => new RecipeListItemViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        PreparationTimeMinutes = r.PreparationTimeMinutes,
                        Difficulty = r.Difficulty.ToString(),
                        CategoryName = r.Category.Name,
                        AuthorName = r.User.UserName ?? "Анонимен",
                        ImageUrl = r.Images.Select(i => i.Url).FirstOrDefault(),
                        Components = r.ComponentRecipes.Select(cr => new RecipeComponentViewModel
                        {
                            Id = cr.Component.Id,
                            Name = cr.Component.Name
                        }).ToList()
                    })
                    .ToListAsync()
            };

            ViewData["SearchTerm"] = component.Name;
            return View("SearchResults", viewModel);
        }
        
    }
}