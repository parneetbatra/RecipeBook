using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Models
{
    public class RecipeUserView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public string Image { get; internal set; }

        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeTimeToComplete { get; set; }
        public string RecipeImage { get; set; }


        public List<RecipeImages> RecipeImages { get; set; }
        public List<Step> Steps { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        
    }
}
