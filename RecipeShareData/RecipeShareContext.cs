using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeShareData.Entities;

namespace RecipeShareData
{
    public class RecipeShareContext : IdentityDbContext<User>
    {

        public DbSet<Entities.Comment> Comments { get; set; }
        public DbSet<Entities.Component> Components { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Entities.Post> Posts { get; set; }
        public DbSet<Entities.Recipe> Recipes { get; set; }


        public RecipeShareContext(DbContextOptions<RecipeShareContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }

}
