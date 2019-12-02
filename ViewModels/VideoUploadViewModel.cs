using System.ComponentModel.DataAnnotations;
using System.Web;

namespace LabWork15.ViewModels
{
    public class VideoUploadViewModel
    {
        [Required, MaxLength(64), Display(Name = "Название")]
        public string Title { get; set; }

        [Required, Display(Name = "Файл")]
        public HttpPostedFileBase File { get; set; }
    }
}