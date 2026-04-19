using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
	public class Category
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;

		public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
	}
}
