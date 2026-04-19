using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
	public class Component
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }

		public ICollection<ComponentRecipe> ComponentRecipes { get; set; } = new List<ComponentRecipe>();
	}
}
