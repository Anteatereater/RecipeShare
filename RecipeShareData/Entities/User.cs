using Microsoft.AspNet.Identity.EntityFramework;

namespace RecipeShareData.Entities
{
    public class User : IdentityUser
    {
        public string? Description { get; set; }

        public ICollection<Post>? Posts { get; set; }
    }
}
