using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The username is required.")]
        [StringLength(100, ErrorMessage = "The username cannot exceed 100 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The comment date is required.")]
        public DateTime CommentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "The content is required.")]
        [StringLength(500, ErrorMessage = "The content cannot exceed 500 characters.")]
        public string Content { get; set; } = string.Empty;

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public Post? Post { get; set; }
    }
}
