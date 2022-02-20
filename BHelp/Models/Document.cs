using System.Collections.Generic;
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
        public string Title { get; set; }
        public string MenuCategory { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        [StringLength(128)] public string OrginatorId { get; set; }
        [NotMapped] public List<SelectListItem> Categories { get; set; }
        [NotMapped] public string ErrorMessage { get; set; }
    }
}