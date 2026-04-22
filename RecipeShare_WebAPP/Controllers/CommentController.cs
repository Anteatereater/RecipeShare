using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShareData;
using RecipeShareData.Entities;
using System.Security.Claims;

namespace RecipeShare_WebAPP.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly RecipeShareContext _context;

        public CommentController(RecipeShareContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid recipeId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Recipe", new { id = recipeId });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Text = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                RecipeId = recipeId,
                UserId = userId!
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId == userId || User.IsInRole("Admin"))
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Recipe", new { id = comment.RecipeId });
        }
    }
}