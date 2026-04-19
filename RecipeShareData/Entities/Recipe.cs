using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Recipe
    {
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public int PreparationTimeMinutes { get; set; }
		public DifficultyLevel Difficulty { get; set; }
		public DateTime CreatedAt { get; set; }

		public string UserId { get; set; } = null!;
		public User User { get; set; } = null!;

		[ForeignKey("Category")]
		public Guid CategoryId { get; set; }
		public Category Category { get; set; } = null!;

		public ICollection<ComponentRecipe> ComponentRecipes { get; set; } = new List<ComponentRecipe>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
		public ICollection<Image> Images { get; set; } = new List<Image>();

	}

	public enum DifficultyLevel
	{
		Ниска,
		Средна,
		Висока
		
	}
}
