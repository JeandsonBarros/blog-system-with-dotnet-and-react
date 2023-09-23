using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.DTOs
{
    public class CommentDto
    {
        [Required(ErrorMessage = "Comentário é obrigatório.")]
        public string CommentText { get; set; }
    }
}