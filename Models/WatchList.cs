using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Models
{
    public class WatchList
    {
        [Key]
        public int WatchId { get; set; }
        public int ItemId { get; set; }
        public string UserId { get; set; }


    }
}
