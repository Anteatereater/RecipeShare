using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Web.ViewModels.ViewModels.Recipes
{
	public class RecipeDetailsViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public int PreparationTimeMinutes { get; set; }
		public string Difficulty { get; set; } = null!;
		public string CategoryName { get; set; } = null!;
		public string AuthorName { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public List<string> ImageUrls { get; set; } = new();
        public List<RecipeComponentViewModel> Components { get; set; } = new();
        public string UserId { get; set; } = null!;

        public class RecipeComponentViewModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
        }
    }
}
