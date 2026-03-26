using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData
{
    public class Post
    {
        public Guid Id { get; set; }
        public string? Text { get; set; }
        public int Likes { get; set; }
        public DateTime CreatetAt { get; set; }

        [ForeignKey("Recipe")]
        public Guid RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        [ForeignKey("User")]
        public Guid UserID { get; set; }
        public User? User { get; set; }
    }
}
