using BatchProcessor.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatchProcessor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
