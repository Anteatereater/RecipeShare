using Microsoft.EntityFrameworkCore;
using RecipeShareData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShareData
{
    internal class RecipeShareDataContext : DbContext
    {

        public DbSet<Entities.Comment> Comments { get; set; }
        public DbSet<Entities.Component> Components { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Entities.Post> Posts { get; set; }
        public DbSet<Entities.Recipe> Recipes { get; set; }
        public DbSet<Entities.User> Users { get; set; }


        public RecipeShareDataContext(DbContextOptions<RecipeShareDataContext> options) : base(options)
        {
        }   




    }

}
