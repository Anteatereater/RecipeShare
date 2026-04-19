using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Comment
    {

		public Guid Id { get; set; }
		public string Text { get; set; } = null!;
		public DateTime CreatedAt { get; set; }

		[ForeignKey("Recipe")]
		public Guid RecipeId { get; set; }
		public Recipe Recipe { get; set; } = null!;

		public string UserId { get; set; } = null!;
		public User User { get; set; } = null!;
    }
}
