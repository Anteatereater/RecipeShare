using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Web.ViewModels.ViewModels.Recipes
{
    public class RecipeCreateViewModel
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        [Range(1, 1440, ErrorMessage = "Времето трябва да бъде поне 1 минута!")]
        public int PreparationTimeMinutes { get; set; }
        public string Difficulty { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageName { get; set; }

        
        public List<SelectListItem> AvailableComponents { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Difficulties { get; set; } = new();

   
        public List<RecipeComponentSelectionViewModel> SelectedComponents { get; set; } = new();
    }

    public class RecipeComponentSelectionViewModel
    {
        public Guid ComponentId { get; set; }
    }
}
