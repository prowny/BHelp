using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DisplayName("Menu Title")]
        public string Title { get; set; }
        [DisplayName("Menu Category")]
        public string MenuCategory { get; set; }
        [DisplayName("File Name")]
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        [StringLength(128)] public string OriginatorId { get; set; }
        [NotMapped] public List<SelectListItem> Categories { get; set; }
        [NotMapped] public string TitleErrorMessage { get; set; }
        [NotMapped] public string FileErrorMessage { get; set; }
        [NotMapped] public string ViewSourceFile { get; set; }
        [NotMapped] public string OriginatorName { get; set; }
    }
}