using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Web.ViewModels.Component;
using RecipeShareData;
using RecipeShareData.Entities;

namespace RecipeShare_WebAPP.Controllers
{
   
    public class ComponentsController : Controller
    {
        private readonly RecipeShareContext _context;

        public ComponentsController(RecipeShareContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new ComponentCreateViewModel();

            model.AvailableComponents = await _context.Components
                .AsNoTracking()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComponentCreateViewModel model)
        {
           
            bool exists = await _context.Components
                .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
              
                ModelState.AddModelError("Name", "Вече съществува съставка с това име!");
            }

            if (!ModelState.IsValid)
            {
                
                model.AvailableComponents = await _context.Components
                    .AsNoTracking()
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();

                return View(model);
            }

            var newComponent = new RecipeShareData.Entities.Component
            {
                Name = model.Name.Trim() 
            };

            _context.Components.Add(newComponent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Create));
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var component = await _context.Components.FindAsync(id);
            if (component == null) return NotFound();

            var isUsed = await _context.ComponentRecipes.AnyAsync(cr => cr.ComponentId == id);
            if (isUsed)
            {
                return RedirectToAction(nameof(Create));
            }

            _context.Components.Remove(component);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Create));
        }
    }
}