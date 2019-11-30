using System.ComponentModel.DataAnnotations;

namespace LabWork15.ViewModels
{
    public class VideoEditViewModel
    {
        [Required, MaxLength(64), Display(Name = "Название")]
        public string Title { get; set; }
    }
}