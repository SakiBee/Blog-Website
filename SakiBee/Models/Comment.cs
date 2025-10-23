using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakiBee.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The Username name is required")]
        [MaxLength(100, ErrorMessage = "The username cannot exceed 100 characters")]
        public string UserName { get; set; }

        [DataType(DataType.Date)]
        [ValidateNever]
        public DateTime CommentDate { get; set; } = DateTime.Now;

        [Required]
        public string Content { get; set; }

        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }
    }
}
