using Microsoft.EntityFrameworkCore;

namespace projekt.Data
{
    public class OrderContext : DbContext
    {

        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {

        }

        public DbSet<projekt.Models.Order> Orders { get; set; }
    }
}
