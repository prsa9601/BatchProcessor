using BatchProcessor.Dtos;

namespace BatchProcessor.Services.Abstractions
{
    public interface IPostLikeService
    {
        Task BulkLikePostsAsync(IEnumerable<PostLikeDto> likes, CancellationToken ct);
    }

}
