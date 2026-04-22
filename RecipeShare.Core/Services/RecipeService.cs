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
using static RecipeShare.Web.ViewModels.ViewModels.Recipes.RecipeDetailsViewModel;

namespace RecipeShare.Core.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly RecipeShareContext _context;

		public RecipeService(RecipeShareContext context)
		{
			_context = context;
		}

        public async Task<RecipeIndexViewModel> GetAllAsync(string? category, string? difficulty, int? maxTime, string? sortOrder = null)
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

            if (maxTime.HasValue && maxTime.Value > 0)
            {
                query = query.Where(r => r.PreparationTimeMinutes <= maxTime.Value);
            }

            query = sortOrder switch
            {
                "time_asc" => query.OrderBy(r => r.PreparationTimeMinutes),
                "time_desc" => query.OrderByDescending(r => r.PreparationTimeMinutes),
                "diff_asc" => query.OrderBy(r => r.Difficulty),
                "diff_desc" => query.OrderByDescending(r => r.Difficulty),
                "name" => query.OrderBy(r => r.Name),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var recipes = await query
                .Select(r => new RecipeListItemViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    PreparationTimeMinutes = r.PreparationTimeMinutes,
                    Difficulty = r.Difficulty.ToString(),
                    CategoryName = r.Category.Name,
                    AuthorName = r.User.UserName ?? "Анонимен",
                    ImageUrl = r.Images.Select(i => i.Url).FirstOrDefault()
                })
                .ToListAsync();

            return new RecipeIndexViewModel
            {
                Recipes = recipes
            };
        }

        public async Task<RecipeDetailsViewModel?> GetByIdAsync(Guid id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Images)
                .Include(r => r.ComponentRecipes)
                    .ThenInclude(cr => cr.Component)
                .Include(r => r.Comments) 
                    .ThenInclude(c => c.User)
                .Include(r => r.Comments).ThenInclude(c => c.User)
               .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return null;

            return new RecipeDetailsViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                PreparationTimeMinutes = recipe.PreparationTimeMinutes,
                Difficulty = recipe.Difficulty.ToString(),
                CategoryName = recipe.Category.Name,
                AuthorName = recipe.User.UserName ?? "Анонимен",
                ImageUrls = recipe.Images.Select(i => i.Url).ToList(),
                CreatedAt = recipe.CreatedAt,
                UserId = recipe.UserId,

                
                Components = recipe.ComponentRecipes.Select(cr => new RecipeComponentViewModel
                {
                    Id = cr.Component.Id,
                    Name = cr.Component.Name
                }).ToList(),


                Comments = recipe.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new RecipeDetailsViewModel.CommentViewModel 
                    {
                     Id = c.Id,
                     Text = c.Text, 
                     AuthorName = c.User.UserName ?? "Анонимен",
                     UserId = c.UserId,
                     CreatedAt = c.CreatedAt
                    }).ToList()
            };
        }

        public async Task<RecipeCreateViewModel> GetCreateModelAsync()
        {
            var model = new RecipeCreateViewModel
            {

                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync(),


                AvailableComponents = await _context.Components
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync(),


                Difficulties = new List<SelectListItem>
        {
            new SelectListItem { Value = "Ниска", Text = "Ниска" },
            new SelectListItem { Value = "Средна", Text = "Средна" },
            new SelectListItem { Value = "Висока", Text = "Висока" }
        }
            };

            return model;
        }

        public async Task CreateAsync(RecipeCreateViewModel model, string userId)
        {
            
            var recipe = new Recipe
            {
                Name = model.Name,
                Description = model.Description,
                PreparationTimeMinutes = model.PreparationTimeMinutes,

               
                Difficulty = Enum.Parse<DifficultyLevel>(model.Difficulty, true),

                CategoryId = model.CategoryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,

                
                ComponentRecipes = new List<ComponentRecipe>(),
                Images = new List<Image>()
            };

            
            if (model.SelectedComponents != null)
            {
                foreach (var item in model.SelectedComponents)
                {
                    recipe.ComponentRecipes.Add(new ComponentRecipe
                    {
                        ComponentId = item.ComponentId,

                        

                    });
                }
            }

           
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                recipe.Images.Add(new Image
                {
                    Url = model.ImageUrl,
                    Name = model.ImageName ?? model.Name
                });
            }

          
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task<RecipeEditViewModel?> GetEditModelAsync(Guid id, string userId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

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
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null || recipe.UserId != userId)
            {
                return null; 
            }

            return new RecipeDetailsViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                AuthorName = recipe.User.UserName ?? "Анонимен"
            };
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

        public async Task<IEnumerable<RecipeListItemViewModel>> GetRecipesByComponentAsync(Guid componentId)
        {
            return await _context.Recipes
                .Where(r => r.ComponentRecipes.Any(cr => cr.ComponentId == componentId))
                .Select(r => new RecipeListItemViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    CategoryName = r.Category.Name,
                    Difficulty = r.Difficulty.ToString(),
                    PreparationTimeMinutes = r.PreparationTimeMinutes,
                    ImageUrl = r.Images.Select(i => i.Url).FirstOrDefault(),
                    AuthorName = r.User.UserName ?? "Unknown"
                })
                .ToListAsync();
        }

        public async Task<bool> EditAsync(Guid id, RecipeEditViewModel model, string userId, bool isAdmin)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return false;
            }

            
            if (recipe.UserId != userId && !isAdmin)
            {
                return false;
            }

            recipe.Name = model.Name;
            recipe.Description = model.Description;
            recipe.PreparationTimeMinutes = model.PreparationTimeMinutes;
            recipe.CategoryId = model.CategoryId;

            if (Enum.TryParse<DifficultyLevel>(model.Difficulty, out var parsedDifficulty))
            {
                recipe.Difficulty = parsedDifficulty;
            }

           
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
              
                var existingImage = recipe.Images.FirstOrDefault();
                if (existingImage != null)
                {
                    existingImage.Url = model.ImageUrl;
                }
                else
                {
                    recipe.Images.Add(new Image { Url = model.ImageUrl, Name = model.Name });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
