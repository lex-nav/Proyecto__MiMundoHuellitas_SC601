using System.Data.Entity;
using MiMundoHuellitas.Models;

namespace MiMundoHuellitas.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")   
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

      
    }
}
