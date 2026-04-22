using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RecipeShare.Web.ViewModels.Component 
{
    public class ComponentCreateViewModel
    {
        
        public string Name { get; set; } = null!;

        
        public List<SelectListItem> AvailableComponents { get; set; } = new List<SelectListItem>();
    }
}