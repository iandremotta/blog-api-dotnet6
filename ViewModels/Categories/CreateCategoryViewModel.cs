using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(30)]
        public string Name { get; set; }
        [Required]
        public string Slug { get; set; }
    }
}