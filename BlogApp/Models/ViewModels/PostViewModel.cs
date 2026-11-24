using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class PostViewModel
    {
        public Post Post { get; set; } = new Post();

        [ValidateNever]
        public SelectList? Categories { get; set; }

        [Required(ErrorMessage = "Please select an image")]

        public IFormFile FeatureImage { get; set; } = null!;
    }
}