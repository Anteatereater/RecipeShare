using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Component>? IngrainedList { get; set; }

        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public List<Post>? Post { get; set; }

        public ICollection<ComponentRecipe>? ComponentRecipes { get; set; }

    }
}
