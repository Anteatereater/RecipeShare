using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Core.Interfaces
{
	public interface IRecipeService
	{
		Task<RecipeIndexViewModel> GetAllAsync(string? category, string? difficulty, int? maxTime);
		Task<RecipeDetailsViewModel?> GetByIdAsync(Guid id);
		Task<RecipeCreateViewModel> GetCreateModelAsync();
		Task CreateAsync(RecipeCreateViewModel model, string userId);
		Task<RecipeEditViewModel?> GetEditModelAsync(Guid id, string userId);
		Task<bool> UpdateAsync(RecipeEditViewModel model, string userId);
		Task<RecipeDetailsViewModel?> GetDeleteModelAsync(Guid id, string userId);
		Task<bool> DeleteAsync(Guid id, string userId);
        Task<IEnumerable<RecipeListItemViewModel>> GetRecipesByComponentAsync(Guid componentId);
        Task<bool> EditAsync(Guid id, RecipeEditViewModel model, string userId, bool isAdmin);
    }
}
