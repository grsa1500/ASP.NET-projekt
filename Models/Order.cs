﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Total { get; set; }
        public DateTime Date { get; set; }
        public string TempId { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt2")]
        public string Shipping { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt3")]
        [Display(Name = "Adress")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt111")]
        [Display(Name = "Postnummer")]
        public string Zip { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt4")]
        [Display(Name = "Ort")]
        public string City { get; set; }

    }
    }

