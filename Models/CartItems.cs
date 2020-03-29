using System;
using System.ComponentModel.DataAnnotations;

namespace projekt.Models
{
    public class CartItems
    {

       [Key]
        public int CartItemId { get; set; }

        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string User { get; set; }

     
    }
}