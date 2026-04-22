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
        public class ComponentInputModel
        {
            public Guid ComponentId { get; set; }
          
        }

        [Required]
		[StringLength(100)]
		public string Name { get; set; } = null!;

		[Required]
		[StringLength(2000)]
		public string Description { get; set; } = null!;

		[Range(1, 1000)]
		public int PreparationTimeMinutes { get; set; }

		[Required]
		public string Difficulty { get; set; } = null!;

		[Required]
		public Guid CategoryId { get; set; }

		public string? ImageUrl { get; set; }
		public string? ImageName { get; set; }

		public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> Difficulties { get; set; } = new List<SelectListItem>();

        public List<ComponentInputModel> SelectedComponents { get; set; } = new();
        public IEnumerable<SelectListItem> AvailableComponents { get; set; } = new List<SelectListItem>();
    }
}
