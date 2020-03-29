using Microsoft.EntityFrameworkCore;

namespace projekt.Data
{
    public class OrderItemContext : DbContext
    {

        public OrderItemContext(DbContextOptions<OrderItemContext> options) : base(options)
        {

        }

        public DbSet<projekt.Models.OrderItem> OrderItems { get; set; }
    }
}
