using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class BlogDto
    {
        [Required(ErrorMessage = "Title is mandatory.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is mandatory.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "State whether it is public.")]
        public bool IsPublic { get; set; }
        public string? HeaderColor { get; set; }
        public string? TitleColor { get; set; }
    }

    [ValidateNever]
    public class BlogDtoNoValidation
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsPublic { get; set; }
        public string? HeaderColor { get; set; }
        public string? TitleColor { get; set; }
    }

}