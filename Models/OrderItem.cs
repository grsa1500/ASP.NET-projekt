using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public string OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

    }
}
