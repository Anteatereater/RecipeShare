using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Interfaces;
using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using RecipeShareData;
using RecipeShareData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Core.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly RecipeShareContext _context;

		public RecipeService(RecipeShareContext context)
		{
			_context = context;
		}

		public async Task<RecipeIndexViewModel> GetAllAsync(string? category, string? difficulty, int? maxTime)
		{
			var query = _context.Recipes
				.Include(r => r.Category)
				.Include(r => r.Images)
				.Include(r => r.User)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(category))
			{
				query = query.Where(r => r.Category.Name == category);
			}

			if (!string.IsNullOrWhiteSpace(difficulty))
			{
				query = query.Where(r => r.Difficulty.ToString() == difficulty);
			}

			if (maxTime.HasValue)
			{
				query = query.Where(r => r.PreparationTimeMinutes <= maxTime.Value);
			}

			var recipes = await query
				.OrderByDescending(r => r.CreatedAt)
				.Select(r => new RecipeListItemViewModel
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					PreparationTimeMinutes = r.PreparationTimeMinutes,
					Difficulty = r.Difficulty.ToString(),
					CategoryName = r.Category.Name,
					AuthorName = r.User.UserName ?? "Unknown",
					ImageUrl = r.Images.Select(i => i.Url).FirstOrDefault(),
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();

			var categories = await _context.Categories
				.OrderBy(c => c.Name)
				.Select(c => c.Name)
				.ToListAsync();

			var difficulties = Enum.GetNames(typeof(DifficultyLevel));

			return new RecipeIndexViewModel
			{
				Recipes = recipes,
				Category = category,
				Difficulty = difficulty,
				MaxTime = maxTime,
				Categories = categories,
				Difficulties = difficulties
			};
		}

		public async Task<RecipeDetailsViewModel?> GetByIdAsync(Guid id)
		{
			return await _context.Recipes
				.Include(r => r.Category)
				.Include(r => r.Images)
				.Include(r => r.User)
				.Where(r => r.Id == id)
				.Select(r => new RecipeDetailsViewModel
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					PreparationTimeMinutes = r.PreparationTimeMinutes,
					Difficulty = r.Difficulty.ToString(),
					CategoryName = r.Category.Name,
					AuthorName = r.User.UserName ?? "Unknown",
					CreatedAt = r.CreatedAt,
					ImageUrls = r.Images.Select(i => i.Url).ToList()
				})
				.FirstOrDefaultAsync();
		}

		public async Task<RecipeCreateViewModel> GetCreateModelAsync()
		{
			return new RecipeCreateViewModel
			{
				Categories = await GetCategorySelectListAsync(),
				Difficulties = GetDifficultySelectList()
			};
		}

		public async Task CreateAsync(RecipeCreateViewModel model, string userId)
		{
			Enum.TryParse<DifficultyLevel>(model.Difficulty, out var difficultyEnum);

			var recipe = new Recipe
			{
				Id = Guid.NewGuid(),
				Name = model.Name,
				Description = model.Description,
				PreparationTimeMinutes = model.PreparationTimeMinutes,
				Difficulty = difficultyEnum,
				CreatedAt = DateTime.UtcNow,
				UserId = userId,
				CategoryId = model.CategoryId
			};

			_context.Recipes.Add(recipe);

			if (!string.IsNullOrWhiteSpace(model.ImageUrl))
			{
				_context.Images.Add(new Image
				{
					Id = Guid.NewGuid(),
					Name = string.IsNullOrWhiteSpace(model.ImageName) ? model.Name : model.ImageName,
					Url = model.ImageUrl,
					RecipeId = recipe.Id
				});
			}

			await _context.SaveChangesAsync();
		}

		public async Task<RecipeEditViewModel?> GetEditModelAsync(Guid id, string userId)
		{
			var recipe = await _context.Recipes
				.Include(r => r.Images)
				.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

			if (recipe == null) return null;

			return new RecipeEditViewModel
			{
				Id = recipe.Id,
				Name = recipe.Name,
				Description = recipe.Description,
				PreparationTimeMinutes = recipe.PreparationTimeMinutes,
				Difficulty = recipe.Difficulty.ToString(),
				CategoryId = recipe.CategoryId,
				ImageUrl = recipe.Images.FirstOrDefault()?.Url,
				ImageName = recipe.Images.FirstOrDefault()?.Name,
				Categories = await GetCategorySelectListAsync(recipe.CategoryId),
				Difficulties = GetDifficultySelectList(recipe.Difficulty.ToString())
			};
		}

		public async Task<bool> UpdateAsync(RecipeEditViewModel model, string userId)
		{
			var recipe = await _context.Recipes
				.Include(r => r.Images)
				.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);

			if (recipe == null) return false;

			Enum.TryParse<DifficultyLevel>(model.Difficulty, out var difficultyEnum);

			recipe.Name = model.Name;
			recipe.Description = model.Description;
			recipe.PreparationTimeMinutes = model.PreparationTimeMinutes;
			recipe.Difficulty = difficultyEnum;
			recipe.CategoryId = model.CategoryId;

			var image = recipe.Images.FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(model.ImageUrl))
			{
				if (image == null)
				{
					_context.Images.Add(new Image
					{
						Id = Guid.NewGuid(),
						Name = string.IsNullOrWhiteSpace(model.ImageName) ? model.Name : model.ImageName,
						Url = model.ImageUrl,
						RecipeId = recipe.Id
					});
				}
				else
				{
					image.Url = model.ImageUrl;
					image.Name = string.IsNullOrWhiteSpace(model.ImageName) ? model.Name : model.ImageName;
				}
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<RecipeDetailsViewModel?> GetDeleteModelAsync(Guid id, string userId)
		{
			return await _context.Recipes
				.Include(r => r.Category)
				.Include(r => r.Images)
				.Include(r => r.User)
				.Where(r => r.Id == id && r.UserId == userId)
				.Select(r => new RecipeDetailsViewModel
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					PreparationTimeMinutes = r.PreparationTimeMinutes,
					Difficulty = r.Difficulty.ToString(),
					CategoryName = r.Category.Name,
					AuthorName = r.User.UserName ?? "Unknown",
					CreatedAt = r.CreatedAt,
					ImageUrls = r.Images.Select(i => i.Url).ToList()
				})
				.FirstOrDefaultAsync();
		}

		public async Task<bool> DeleteAsync(Guid id, string userId)
		{
			var recipe = await _context.Recipes
				.Include(r => r.Images)
				.Include(r => r.Comments)
				.Include(r => r.ComponentRecipes)
				.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

			if (recipe == null) return false;

			_context.Images.RemoveRange(recipe.Images);
			_context.Comments.RemoveRange(recipe.Comments);
			_context.RemoveRange(recipe.ComponentRecipes);
			_context.Recipes.Remove(recipe);

			await _context.SaveChangesAsync();
			return true;
		}

		private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync(Guid? selectedId = null)
		{
			return await _context.Categories
				.OrderBy(c => c.Name)
				.Select(c => new SelectListItem
				{
					Value = c.Id.ToString(),
					Text = c.Name,
					Selected = selectedId.HasValue && c.Id == selectedId.Value
				})
				.ToListAsync();
		}

		private IEnumerable<SelectListItem> GetDifficultySelectList(string? selected = null)
		{
			return Enum.GetNames(typeof(DifficultyLevel))
				.Select(d => new SelectListItem
				{
					Value = d,
					Text = d,
					Selected = d == selected
				})
				.ToList();
		}
	}
}
