using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movies_Project.Models;

namespace Movies_Project.Data
{
    public class Movies_ProjectContext : DbContext
    {
        public Movies_ProjectContext (DbContextOptions<Movies_ProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Movies_Project.Models.Movie> Movie { get; set; } = default!;
        public DbSet<Movies_Project.Models.Manager> Manager { get; set; } = default!;
        public DbSet<Movies_Project.Models.Genre> Genre { get; set; } = default!;
        public DbSet<Movies_Project.Models.Director> Director { get; set; } = default!;
    }
}
