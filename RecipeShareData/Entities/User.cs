using Microsoft.AspNetCore.Identity;

namespace RecipeShareData.Entities
{
    public class User : IdentityUser
    {

		public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	}
}
