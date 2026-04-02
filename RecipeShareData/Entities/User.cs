using Microsoft.AspNetCore.Identity;

namespace RecipeShareData.Entities
{
    public class User : IdentityUser
    {
        public string? Description { get; set; }

        public ICollection<Post>? Posts { get; set; } = new List<Post>();
    }
}
