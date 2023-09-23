using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class BlogDto
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Assunto é obrigatório.")]
        public string Matter { get; set; }
        [Required(ErrorMessage = "Informe se é público.")]
        public bool IsPublic { get; set; }
        public string? ColorPrimary { get; set; }
        public string? ColorSecondary { get; set; }
    }

    [ValidateNever]
    public class BlogDtoNoValidation
    {
        public string? Name { get; set; }
        public string? Matter { get; set; }
        public bool? IsPublic { get; set; }
        public string? ColorPrimary { get; set; }
        public string? ColorSecondary { get; set; }
    }

}