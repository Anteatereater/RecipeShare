using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Interfaces;
using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using RecipeShareData;
using RecipeShareData.Entities;

namespace RecipeShare_WebAPP.Controllers
{
	public class RecipesController : Controller
	{
		private readonly IRecipeService _recipeService;
		private readonly UserManager<User> _userManager;

		public RecipesController(IRecipeService recipeService, UserManager<User> userManager)
		{
			_recipeService = recipeService;
			_userManager = userManager;
		}

		// GET: Recipes
		public async Task<IActionResult> Index(string? category, string? difficulty, int? maxTime)
		{
			var model = await _recipeService.GetAllAsync(category, difficulty, maxTime);
			return View(model);
		}

		// GET: Recipes/Details/{id}
		public async Task<IActionResult> Details(Guid id)
		{
			var model = await _recipeService.GetByIdAsync(id);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		// GET: Recipes/Create
		[Authorize]
		public async Task<IActionResult> Create()
		{
			var model = await _recipeService.GetCreateModelAsync();
			return View(model);
		}

		// POST: Recipes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Create(RecipeCreateViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var reloadModel = await _recipeService.GetCreateModelAsync();
				model.Categories = reloadModel.Categories;
				model.Difficulties = reloadModel.Difficulties;

				return View(model);
			}

			var userId = _userManager.GetUserId(User)!;
			await _recipeService.CreateAsync(model, userId);

			return RedirectToAction(nameof(Index));
		}

		// GET: Recipes/Edit/{id}
		[Authorize]
		public async Task<IActionResult> Edit(Guid id)
		{
			var userId = _userManager.GetUserId(User)!;
			var model = await _recipeService.GetEditModelAsync(id, userId);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		// POST: Recipes/Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Edit(RecipeEditViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var reloadModel = await _recipeService.GetEditModelAsync(model.Id, _userManager.GetUserId(User)!);

				if (reloadModel == null)
				{
					return NotFound();
				}

				model.Categories = reloadModel.Categories;
				model.Difficulties = reloadModel.Difficulties;

				return View(model);
			}

			var userId = _userManager.GetUserId(User)!;
			var success = await _recipeService.UpdateAsync(model, userId);

			if (!success)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}

		// GET: Recipes/Delete/{id}
		[Authorize]
		public async Task<IActionResult> Delete(Guid id)
		{
			var userId = _userManager.GetUserId(User)!;
			var model = await _recipeService.GetDeleteModelAsync(id, userId);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		// POST: Recipes/Delete/{id}
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			var userId = _userManager.GetUserId(User)!;
			var success = await _recipeService.DeleteAsync(id, userId);

			if (!success)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
