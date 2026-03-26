using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Ingrained>? IngrainedList { get; set; }

        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public List<Post>? Post { get; set; }
        
    }
}
