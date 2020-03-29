using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Models
{
    public class Users
    {
        [Required]
        [Key]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        [Display(Name = "E-postadress")]
        [DataType(DataType.EmailAddress)]
        public string Mail { get; set; }
        [Required(ErrorMessage = "Fält ej ifyllt")]
        [Display(Name = "Lösenord")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Bekräfta lösenord")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Lösenorden matchar inte.")]
        public string PasswordConfirm { get; set; }
    }
}
