using Microsoft.EntityFrameworkCore;
using ShortUrl.BackEnd.Models;

namespace ShortUrl.BackEnd.Data
{
    public class UrlDbContext : DbContext
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }
        public virtual DbSet<UrlManagement> Urls { get; set; }
    }
}
