using System.ComponentModel.DataAnnotations;

namespace LabWork15.ViewModels
{
    public class VideoWatchViewModel
    {
        [Required, MaxLength(64), Display(Name = "Название")]
        public string Title { get; set; }

        [Required, Editable(false), MaxLength(128), Display(Name = "Имя файла")]
        public string FileName { get; set; }
    }
}