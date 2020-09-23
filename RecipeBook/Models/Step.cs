﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBook.Models
{
    public class Step
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Step Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Order Number")]
        public int OrderNumber { get; set; }

        // remove this column 
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        [ForeignKey("Recipe")]
        public int RecipeId  { get; set; }

        public virtual Recipe Recipe { get; set; }
    }
}
