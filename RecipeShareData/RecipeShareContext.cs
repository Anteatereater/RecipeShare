using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeShareData.Entities;

namespace RecipeShareData
{
   
    public class RecipeShareContext : IdentityDbContext<User>
    {
		public DbSet<Category> Categories { get; set; }
		public DbSet<Comment> Comments { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
		public DbSet<ComponentRecipe> ComponentRecipes { get; set; }


		public RecipeShareContext(DbContextOptions<RecipeShareContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
						
			base.OnModelCreating(builder);

			builder.Entity<ComponentRecipe>()
				.HasKey(cr => new { cr.RecipeId, cr.ComponentId });

			builder.Entity<ComponentRecipe>()
				.HasOne(cr => cr.Recipe)
				.WithMany(r => r.ComponentRecipes)
				.HasForeignKey(cr => cr.RecipeId);

			builder.Entity<ComponentRecipe>()
				.HasOne(cr => cr.Component)
				.WithMany(c => c.ComponentRecipes)
				.HasForeignKey(cr => cr.ComponentId);

			builder.Entity<Recipe>()
				.HasOne(r => r.User)
				.WithMany(u => u.Recipes)
				.HasForeignKey(r => r.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Comment>()
				.HasOne(c => c.User)
				.WithMany(u => u.Comments)
				.HasForeignKey(c => c.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Recipe>()
				.Property(r => r.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Entity<Category>()
				.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(50);

            builder.Entity<Comment>()
                .Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(500);

            builder.Entity<Component>()
				.HasIndex(c => c.Name)
			    .IsUnique();

        }
    }

}
