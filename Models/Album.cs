using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Models
{
    public class Album
    {

        public int AlbumId { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        [Display(Name = "Namn")]
        public  string Name { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Artist { get; set; }
        [Display(Name = "År")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public int Year { get; set; }
        [Display(Name = "Skivbolag")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Label { get; set; }
        [Display(Name = "Beskrivning")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Description { get; set; }
        [Display(Name = "Genre")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Genre { get; set; }
        [Display(Name = "Bild URL")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Image { get; set; }
        [Display(Name = "Längd")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public string Length { get; set; }
        [Display(Name = "Pris")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public int Price { get; set; }
        [Display(Name = "Antal på lager")]
        public int Quantity { get; set; }
        [Display(Name = "Försäljningar")]
        [Required(ErrorMessage = "Fält ej ifyllt")]
        public int Sales { get; set; }
        [Display(Name = "Rekommenderad")]
        public string Recommendation { get; set; }

    }
}
