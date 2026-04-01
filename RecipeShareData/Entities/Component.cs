using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Component
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }

        public ICollection<ComponentRecipe>? ComponentRecipes { get; set; }
    }

    public class Ingrained : Component
    {
        public int CaloriesPerKg { get; set; }
        public int ProteinPerKg { get; set; }
        public int FibrePerKg { get; set; }

    }

    public class Seasoning : Component
    {

    }
}
