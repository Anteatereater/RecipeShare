using Microsoft.EntityFrameworkCore;
using RecipeShareData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Seed
{
	public static class CategorySeeder
	{
		public static async Task SeedAsync(RecipeShareContext context)
		{
			if (await context.Categories.AnyAsync())
			{
				return; 
			}

			var categories = new List<Category>
			{
				new Category { Id = Guid.NewGuid(), Name = "Супи" },
				new Category { Id = Guid.NewGuid(), Name = "Основни ястия" },
				new Category { Id = Guid.NewGuid(), Name = "Салати" },
				new Category { Id = Guid.NewGuid(), Name = "Десерти" },
				new Category { Id = Guid.NewGuid(), Name = "Закуски" },
				new Category { Id = Guid.NewGuid(), Name = "Напитки" }
			};

			await context.Categories.AddRangeAsync(categories);
			await context.SaveChangesAsync();
		}
	}
}
