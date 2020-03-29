using Microsoft.EntityFrameworkCore;
using projekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projekt.Data
{
    public class UsersContext : DbContext
    {

        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {

        }

        public DbSet<projekt.Models.Users> Users { get; set; }
    }
}
