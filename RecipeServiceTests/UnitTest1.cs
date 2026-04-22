using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Services;
using RecipeShare.Web.ViewModels.Component;
using RecipeShare.Web.ViewModels.ViewModels.Recipes;
using RecipeShare_WebAPP.Controllers;
using RecipeShare_WebAPP.Models.Home;
using RecipeShareData;
using RecipeShareData.Entities;
using Xunit;

/*
Ниска,
Средна,
Висока*/
namespace RecipeShare.Tests
{
    public class RecipeServiceTests
    {
        [Fact]
        public async Task Comment_Add_RedirectsToDetails_WhenContentIsEmpty()
        {
            var context = GetDatabaseContext();
            var controller = new CommentController(context);
            var recipeId = Guid.NewGuid();

            var result = await controller.Add(recipeId, "");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
        }

     
        private void MockUser(Controller controller, string userId)
        {
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
            {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
    }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Comment_Add_SavesComment_WhenValid()
        {
            var context = GetDatabaseContext();
            var controller = new CommentController(context);
            MockUser(controller, "user-123");
            var recipeId = Guid.NewGuid();

            await controller.Add(recipeId, "Много вкусна рецепта!");

            var comment = await context.Comments.FirstOrDefaultAsync();
            Assert.NotNull(comment);
            Assert.Equal("Много вкусна рецепта!", comment.Text);
            Assert.Equal("user-123", comment.UserId);
        }

       
        [Fact]
        public async Task GetAllAsync_ShouldFilterByMaxTime()
        {
            var context = GetDatabaseContext();
            var cat = new Category { Name = "C" };
            context.Recipes.Add(new Recipe { Name = "Fast", PreparationTimeMinutes = 15, Category = cat, UserId = "1" });
            context.Recipes.Add(new Recipe { Name = "Slow", PreparationTimeMinutes = 120, Category = cat, UserId = "1" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetAllAsync(null, null, 30);

            Assert.Single(result.Recipes);
            Assert.Equal("Fast", result.Recipes.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByDifficulty()
        {
            var context = GetDatabaseContext();
            var cat = new Category { Name = "C" };
            context.Recipes.Add(new Recipe { Name = "Hard", Difficulty = DifficultyLevel.Висока, Category = cat, UserId = "1" });
            context.Recipes.Add(new Recipe { Name = "Easy", Difficulty = DifficultyLevel.Ниска, Category = cat, UserId = "1" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetAllAsync(null, null, null, "difficulty");

            Assert.Equal("Easy", result.Recipes.First().Name);
        }
        [Fact]
        public async Task Create_Get_ReturnsViewWithComponents()
        {
            var context = GetDatabaseContext();
            context.Components.Add(new Component { Name = "Захар" });
            await context.SaveChangesAsync();
            var controller = new ComponentsController(context);

            var result = await controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ComponentCreateViewModel>(viewResult.ViewData.Model);
            Assert.Single(model.AvailableComponents);
        }

        [Fact]
        public async Task Create_Post_ReturnsError_WhenComponentExists()
        {
            var context = GetDatabaseContext();
            context.Components.Add(new Component { Name = "Сол" });
            await context.SaveChangesAsync();
            var controller = new ComponentsController(context);

            var model = new ComponentCreateViewModel { Name = "сол" };

            var result = await controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Name"));
        }

        [Fact]
        public async Task Create_Post_SavesNewComponent_WhenValid()
        {
            var context = GetDatabaseContext();
            var controller = new ComponentsController(context);
            var model = new ComponentCreateViewModel { Name = "Пипер" };

            var result = await controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(await context.Components.AnyAsync(c => c.Name == "Пипер"));
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenComponentDoesNotExist()
        {
            var context = GetDatabaseContext();
            var controller = new ComponentsController(context);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_RedirectsWithoutDeleting_IfUsedInRecipe()
        {
            var context = GetDatabaseContext();
            var compId = Guid.NewGuid();
            context.Components.Add(new Component { Id = compId, Name = "Брашно" });
            context.ComponentRecipes.Add(new ComponentRecipe { ComponentId = compId, RecipeId = Guid.NewGuid() });
            await context.SaveChangesAsync();

            var controller = new ComponentsController(context);

            var result = await controller.Delete(compId);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(await context.Components.AnyAsync(c => c.Id == compId));
        }

        [Theory]
        [InlineData("Dessert", "Easy", 30, "name")]
        [InlineData(null, "Hard", null, "time_desc")]
        [InlineData("Soup", null, 60, "difficulty")]
        public async Task GetAllAsync_ShouldCoverAllFiltersAndSorting(string cat, string diff, int? time, string sort)
        {
            var context = GetDatabaseContext();
            var category = new Category { Name = "Dessert" };
            context.Recipes.Add(new Recipe { Name = "A", Category = category, Difficulty = DifficultyLevel.Ниска, PreparationTimeMinutes = 20, UserId = "1" });
            context.Recipes.Add(new Recipe { Name = "B", Category = new Category { Name = "Soup" }, Difficulty = DifficultyLevel.Висока, PreparationTimeMinutes = 50, UserId = "1" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetAllAsync(cat, diff, time, sort);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldHandleMissingImagesAndComponents()
        {
            var context = GetDatabaseContext();
            var service = new RecipeService(context);
            var model = new RecipeCreateViewModel
            {
                Name = "Test",
                Difficulty = "Easy",
                CategoryId = Guid.NewGuid(),
                SelectedComponents = new List<RecipeComponentSelectionViewModel> { new() { ComponentId = Guid.NewGuid() } }
            };

            await service.CreateAsync(model, "user1");

            var saved = await context.Recipes.Include(r => r.Images).FirstAsync();
            Assert.Empty(saved.Images);
        }

        [Fact]
        public async Task UpdateAsync_ShouldFailIfUserIsNotOwner()
        {
            var context = GetDatabaseContext();
            var id = Guid.NewGuid();
            context.Recipes.Add(new Recipe { Id = id, UserId = "Owner" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.UpdateAsync(new RecipeEditViewModel { Id = id }, "Stranger");

            Assert.False(result);
        }

        [Fact]
        public async Task EditAsync_ShouldUpdateImage_WhenExistingImageExists()
        {
            var context = GetDatabaseContext();
            var id = Guid.NewGuid();
            var recipe = new Recipe { Id = id, UserId = "1", CategoryId = Guid.NewGuid() };
            recipe.Images.Add(new Image { Url = "old.jpg" });
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var model = new RecipeEditViewModel { Id = id, Name = "New", ImageUrl = "new.jpg", Difficulty = "Hard" };
            await service.EditAsync(id, model, "1", true);

            var img = await context.Images.FirstAsync();
            Assert.Equal("new.jpg", img.Url);
        }

        [Fact]
        public async Task GetEditModelAsync_ShouldReturnModel_WhenUserIsOwner()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var userId = "owner123";
            context.Recipes.Add(new Recipe { Id = recipeId, Name = "Стара", UserId = userId, Category = new Category { Name = "C" } });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetEditModelAsync(recipeId, userId);

            Assert.NotNull(result);
            Assert.Equal("Стара", result.Name);
        }

        [Fact]
        public async Task GetEditModelAsync_ShouldReturnNull_WhenUserIsNotOwner()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            context.Recipes.Add(new Recipe { Id = recipeId, Name = "Чужда", UserId = "Owner" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetEditModelAsync(recipeId, "Stranger");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDeleteModelAsync_ShouldReturnModel_WhenExists()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var userId = "owner";
            context.Recipes.Add(new Recipe { Id = recipeId, Name = "За триене", UserId = userId, User = new User { UserName = "Pesho" } });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetDeleteModelAsync(recipeId, userId);

            Assert.NotNull(result);
            Assert.Equal("За триене", result.Name);
        }

        [Fact]
        public async Task GetRecipesByComponentAsync_ShouldReturnList()
        {
            var context = GetDatabaseContext();
            var compId = Guid.NewGuid();
            var recipe = new Recipe { Name = "Рецепта с компонент", UserId = "1", Category = new Category { Name = "C" } };
            recipe.ComponentRecipes.Add(new ComponentRecipe { ComponentId = compId });
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetRecipesByComponentAsync(compId);

            Assert.Single(result);
            Assert.Equal("Рецепта с компонент", result.First().Name);
        }

        [Fact]
        public async Task EditAsync_ShouldReturnFalse_WhenRecipeNotFound()
        {
            var context = GetDatabaseContext();
            var service = new RecipeService(context);
            var result = await service.EditAsync(Guid.NewGuid(), new RecipeEditViewModel(), "1", true);

            Assert.False(result);
        }

        [Fact]
        public async Task EditAsync_ShouldUpdateAllFields_WhenAuthorized()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            context.Categories.Add(new Category { Id = catId, Name = "Нова" });
            var recipe = new Recipe { Id = recipeId, Name = "Старо", UserId = "1", CategoryId = catId };
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            var service = new RecipeService(context);
            var model = new RecipeEditViewModel
            {
                Id = recipeId,
                Name = "Ново",
                Description = "Деск",
                PreparationTimeMinutes = 10,
                Difficulty = "Easy",
                CategoryId = catId
            };

            var result = await service.EditAsync(recipeId, model, "1", false);

            Assert.True(result);
            var updated = await context.Recipes.FindAsync(recipeId);
            Assert.Equal("Ново", updated.Name);
            Assert.Equal(DifficultyLevel.Ниска, updated.Difficulty);
        }

        [Fact]
        public async Task EditAsync_ShouldUpdateImage_WhenUrlProvided()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var recipe = new Recipe { Id = recipeId, Name = "T", UserId = "1", CategoryId = Guid.NewGuid() };
            recipe.Images.Add(new Image { Url = "old.jpg" });
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            var service = new RecipeService(context);
            var model = new RecipeEditViewModel { Id = recipeId, Name = "T", ImageUrl = "new.jpg" };

            await service.EditAsync(recipeId, model, "1", true);

            var updatedImage = await context.Images.FirstAsync(i => i.RecipeId == recipeId);
            Assert.Equal("new.jpg", updatedImage.Url);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenRecipeNotFound()
        {
            var context = GetDatabaseContext();
            var service = new RecipeService(context);
            var result = await service.DeleteAsync(Guid.NewGuid(), "1");
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenUserIsNotOwner()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            context.Recipes.Add(new Recipe { Id = recipeId, UserId = "RealOwner" });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.UpdateAsync(new RecipeEditViewModel { Id = recipeId }, "HackerId");

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByTime_WhenSortOrderIsProvided()
        {
            var context = GetDatabaseContext();
            context.Recipes.Add(new Recipe { Name = "Бърза", PreparationTimeMinutes = 10, UserId = "1", Category = new Category { Name = "C" } });
            context.Recipes.Add(new Recipe { Name = "Бавна", PreparationTimeMinutes = 100, UserId = "1", Category = new Category { Name = "C" } });
            await context.SaveChangesAsync();
            var service = new RecipeService(context);

            var result = await service.GetAllAsync(null, null, null, "time_desc");

            Assert.Equal("Бавна", result.Recipes.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByCategoryAndDifficulty()
        {
            var context = GetDatabaseContext();
            var cat1 = new Category { Id = Guid.NewGuid(), Name = "Супи" };
            var cat2 = new Category { Id = Guid.NewGuid(), Name = "Десерти" };

            context.Recipes.Add(new Recipe { Id = Guid.NewGuid(), Name = "Леща", Category = cat1, Difficulty = DifficultyLevel.Ниска, UserId = "1" });
            context.Recipes.Add(new Recipe { Id = Guid.NewGuid(), Name = "Торта", Category = cat2, Difficulty = DifficultyLevel.Висока, UserId = "1" });
            await context.SaveChangesAsync();

            var service = new RecipeService(context);

            var result = await service.GetAllAsync("Супи", "Easy", null);

            Assert.Single(result.Recipes);
            Assert.Equal("Леща", result.Recipes.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeComponentsAndImages()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var recipe = new Recipe
            {
                Id = recipeId,
                Name = "Тест",
                UserId = "1",
                Category = new Category { Name = "Кат" },
                User = new User { UserName = "TestUser" }
            };
            recipe.Images.Add(new Image { Url = "test.jpg", Name = "Img" });

            var component = new Component { Id = Guid.NewGuid(), Name = "Захар" };
            recipe.ComponentRecipes.Add(new ComponentRecipe { Component = component });

            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            var service = new RecipeService(context);

            var result = await service.GetByIdAsync(recipeId);

            Assert.NotNull(result);
            Assert.Single(result.ImageUrls);
            Assert.Single(result.Components);
            Assert.Equal("Захар", result.Components.First().Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldCorrectlySaveRecipeWithComponents()
        {
            var context = GetDatabaseContext();
            var service = new RecipeService(context);
            var categoryId = Guid.NewGuid();
            context.Categories.Add(new Category { Id = categoryId, Name = "Тест" });
            await context.SaveChangesAsync();

            var model = new RecipeCreateViewModel
            {
                Name = "Мусака",
                Description = "Описание...",
                PreparationTimeMinutes = 60,
                Difficulty = "Medium",
                CategoryId = categoryId,
                ImageUrl = "mousaka.jpg",
                SelectedComponents = new List<RecipeComponentSelectionViewModel>
                {
                    new RecipeComponentSelectionViewModel { ComponentId = Guid.NewGuid() }
                }
            };

            await service.CreateAsync(model, "user-id-123");

            var saved = await context.Recipes.Include(r => r.ComponentRecipes).FirstAsync();
            Assert.Equal("Мусака", saved.Name);
            Assert.Equal("user-id-123", saved.UserId);
            Assert.Single(saved.ComponentRecipes);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenUserIsOwner()
        {
            var context = GetDatabaseContext();
            var recipeId = Guid.NewGuid();
            var userId = "ownerId";

            context.Recipes.Add(new Recipe { Id = recipeId, Name = "За триене", UserId = userId });
            await context.SaveChangesAsync();

            var service = new RecipeService(context);

            var result = await service.DeleteAsync(recipeId, userId);

            Assert.True(result);
            Assert.Empty(context.Recipes);
        }

        private RecipeShareContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<RecipeShareContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RecipeShareContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}