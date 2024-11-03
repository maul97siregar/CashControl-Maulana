using System.Collections.Generic;
using testMufg.Models;
using Microsoft.EntityFrameworkCore;

namespace testMufg.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<CashTransaction> CashTransaction { get; set; }

    }
}
