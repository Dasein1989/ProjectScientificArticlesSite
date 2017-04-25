using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string AuthorEmail { get; set; }

        [Required]
        public DateTime PostTime { get; set; }

        [Required]
        public string Content { get; set; }

        public string EditMessage { get; set; }
    }
}