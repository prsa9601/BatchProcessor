using BatchProcessor.Data;
using BatchProcessor.Dtos;
using BatchProcessor.Entities;
using BatchProcessor.Services.Abstractions;

namespace BatchProcessor.Services.DataBase
{
    public class PostLikeService : IPostLikeService
    {
        private readonly AppDbContext _context;

        public PostLikeService(AppDbContext context) => _context = context;

        public async Task BulkLikePostsAsync(IEnumerable<PostLikeDto> likes, CancellationToken ct)
        {
            var entities = likes.Select(d => new PostLike
            {
                Id= Guid.NewGuid(),
                PostId = d.PostId,
                UserId = d.UserId,
                LikedAt = d.LikedAt
            });

            // برای تعداد بالا، حتماً از EF Core Bulk Extensions استفاده کنید
            await _context.PostLikes.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
