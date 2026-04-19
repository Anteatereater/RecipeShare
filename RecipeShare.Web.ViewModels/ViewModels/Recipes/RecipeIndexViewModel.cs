using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Web.ViewModels.ViewModels.Recipes
{
	public class RecipeIndexViewModel
	{
		public IEnumerable<RecipeListItemViewModel> Recipes { get; set; } = new List<RecipeListItemViewModel>();

		public string? Category { get; set; }
		public string? Difficulty { get; set; }
		public int? MaxTime { get; set; }

		public IEnumerable<string> Categories { get; set; } = new List<string>();
		public IEnumerable<string> Difficulties { get; set; } = new List<string>();
	}
}
