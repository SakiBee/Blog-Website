using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SakiBee.Models.ViewModels
{
    public class PostViewModel
    {
        public Post Post { get; set; } = new Post();

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IFormFile? FeatureImage { get; set; }
    }
}
