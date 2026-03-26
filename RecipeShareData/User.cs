using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.ComponentModel.DataAnnotations;

namespace RecipeShareData
{
    public class User: IdentityUser
    {
        public string? Description { get; set; }

        public ICollection<Post>? Posts { get; set; }
    }
}
