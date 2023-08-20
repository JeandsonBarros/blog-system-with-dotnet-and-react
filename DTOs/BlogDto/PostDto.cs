using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class PostDto
    {
        public string Title { get; set; }
        public IFormFile? CoverFile { get; set; }
        public string Text { get; set; }
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