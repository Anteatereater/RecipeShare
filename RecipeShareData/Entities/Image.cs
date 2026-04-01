using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Image
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Link { get; set; }

        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public Post? Post { get; set; }

    }
}
