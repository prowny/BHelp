﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int ClientId { get; set; }

        [DisplayName("Delivery Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

        public string Notes { get; set; }
        public int? FullBags { get; set; }
        public int? HalfBags { get; set; }
        public int? KIdSnacks { get; set; }
        public int? GiftCards { get; set; }

        [StringLength(128)]
        public string ODId { get; set; }

        public DateTime? DateDelivered { get; set; }
    }
}