using Microsoft.EntityFrameworkCore;

namespace MoviesWebApp.Models
{
    public class AppDBContext:DbContext
    {
        public AppDBContext( DbContextOptions<AppDBContext> options) :base(options)
        {

        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set; }
    }
}
