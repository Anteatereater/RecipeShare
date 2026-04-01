using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string? text { get; set; }
        public DateTime CreatetAt { get; set; }

        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public Post? Post { get; set; }


    }
}
