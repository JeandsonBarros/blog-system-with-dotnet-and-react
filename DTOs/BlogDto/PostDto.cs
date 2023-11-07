using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class PostDto
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Subtitle is required.")]
        public string Subtitle { get; set; }
        public IFormFile? CoverFile { get; set; }
        [Required(ErrorMessage = "Text is required.")]
        public string Text { get; set; }
        [Required(ErrorMessage = "State whether it is public.")]
        public bool IsPublic { get; set; }
    }

    [ValidateNever]
    public class PostDtoNoValidation
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public IFormFile? CoverFile { get; set; }
        public string? Text { get; set; }
        public bool? IsPublic { get; set; }
    }
}