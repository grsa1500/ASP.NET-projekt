using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using projekt.Models;

namespace projekt.Data
{
    public class CartItemsContext : DbContext
    {

        public CartItemsContext(DbContextOptions<CartItemsContext> options) : base(options)

        {

        }


    public DbSet<projekt.Models.CartItems> CartItems { get; set; }


    public DbSet<projekt.Models.Order> Order { get; set; }
}
    
    
}
