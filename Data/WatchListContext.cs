using Microsoft.EntityFrameworkCore;
using projekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Data
{
    public class WatchListContext : DbContext
    {

        public WatchListContext(DbContextOptions<WatchListContext> options) : base(options)
        {

        }

        public DbSet<WatchList> WatchList { get; set; }
    }
}
