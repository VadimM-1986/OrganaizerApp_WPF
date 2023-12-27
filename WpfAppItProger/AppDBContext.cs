using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WpfAppItProger.Models;

namespace WpfAppItProger
{
    internal class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("Server=localHost; Database=modul_14; Port=3306; User=root; Password=root");
        }
    }
}
