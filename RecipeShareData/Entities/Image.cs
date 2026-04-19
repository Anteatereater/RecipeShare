using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Image
    {
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Url { get; set; } = null!;

		[ForeignKey("Recipe")]
		public Guid RecipeId { get; set; }
		public Recipe Recipe { get; set; } = null!;

	}
}
