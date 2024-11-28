using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WebProject.Models;

namespace WebProject.DataConnection
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Brands> Brands { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Orders> Orders  { get; set; }
        public DbSet<Order_Details> Order_Details { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<RegisterRequest> RegisterRequest { get; set; }
    }

}
