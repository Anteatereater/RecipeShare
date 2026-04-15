using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class IngrainedRecipe
    {
        [ForeignKey("Recipe")]
        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        [ForeignKey("Ingrained")]
        public Guid IngrainedId { get; set; }
        public Ingrained Ingrained { get; set; } = null!;

        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
    }


}
