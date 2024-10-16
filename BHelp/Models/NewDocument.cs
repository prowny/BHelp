﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class NewDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DisplayName("Menu Title"), StringLength(128)]
        public string Title { get; set; }

        [DisplayName("Menu Category"), StringLength(128)]
        public string MenuCategory { get; set; }

        [DisplayName("File Name"), StringLength(256)]
        public string FileName { get; set; }
        [StringLength(128)] public string OriginatorId { get; set; }
        [NotMapped] public List<SelectListItem> Categories { get; set; }
        [NotMapped] public string TitleErrorMessage { get; set; }
        [NotMapped] public string FileErrorMessage { get; set; }
    }
}