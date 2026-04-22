using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
	public class ComponentRecipe
	{
		[ForeignKey("Recipe")]
		public Guid RecipeId { get; set; }
		public Recipe Recipe { get; set; } = null!;

		[ForeignKey("Component")]
		public Guid ComponentId { get; set; }
		public Component Component { get; set; } = null!;

	}
}
