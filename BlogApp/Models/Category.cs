using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is required.")]
        [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "The description cannot exceed 250 characters.")]
        [Required(ErrorMessage = "The description is required.")]
        public string Description { get; set; } = string.Empty;

        public ICollection<Post>? Posts { get; set; }
    }
}
