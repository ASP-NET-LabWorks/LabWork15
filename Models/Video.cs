using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace LabWork15.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(64), Display(Name = "Название")]
        public string Title { get; set; }

        [Required, Editable(false), MaxLength(128), Display(Name = "Имя файла")]
        public string FileName { get; set; }

        [Required, Editable(false), DataType(DataType.Date), Display(Name = "Дата загрузки")]
        public DateTime UploadDate { get; set; }
    }

    public class VideoDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
    }
}