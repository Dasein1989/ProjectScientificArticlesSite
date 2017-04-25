using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class NewsArticleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Author { get; set; }

        [Required(ErrorMessage = "Original source field is required.")]
        public string OriginalSource { get; set; }
    }
}