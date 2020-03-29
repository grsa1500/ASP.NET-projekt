using Microsoft.EntityFrameworkCore;
using projekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Data
{
    public class AlbumContext : DbContext
    {

        public AlbumContext(DbContextOptions<AlbumContext> options) : base(options)
        {

        }

        public DbSet<Album> Albums { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WatchList> WatchList { get; set; }

    }
}
