using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string text { get; set; }
        public DateTime CreatetAt { get; set; }


    }
}
