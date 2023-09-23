using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class PostDto
    {
        [Required(ErrorMessage = "Título é obrigatório.")]
        public string Title { get; set; }
        public IFormFile? CoverFile { get; set; }
        [Required(ErrorMessage = "Texto é obrigatório.")]
        public string Text { get; set; }
        [Required(ErrorMessage = "Informe se é público.")]
        public bool IsPublic { get; set; }
    }

    [ValidateNever]
    public class PostDtoNoValidation
    {
        public string? Title { get; set; }
        public IFormFile? CoverFile { get; set; }
        public string? Text { get; set; }
        public bool? IsPublic { get; set; }
    }
}