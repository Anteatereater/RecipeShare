using RecipeShareData.Entities;

namespace RecipeShare_WebAPP.Models.Home
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Recipe> LatestRecipes { get; set; } = new List<Recipe>();
    }
}
