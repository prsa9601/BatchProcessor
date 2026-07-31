using BatchProcessor.Data;
using BatchProcessor.Dtos;
using BatchProcessor.Entities;
using BatchProcessor.Models;
using BatchProcessor.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BatchProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IBatchProcessor<PostLikeDto> _likeProcessor;
        private readonly AppDbContext _appDbContext;

        public PostController(IBatchProcessor<PostLikeDto> likeProcessor, AppDbContext appDbContext)
        {
            _likeProcessor = likeProcessor;
            _appDbContext = appDbContext;
        }

        [HttpPost("{postId}/like")]
        public IActionResult LikePost(Guid postId)
        {
            Guid userId = Guid.NewGuid();
            var likeDto = new PostLikeDto { PostId = postId, UserId = userId, LikedAt = DateTime.UtcNow };
            _likeProcessor.Add(likeDto); // فقط اضافه به صف
            var rawUrl = $"/api/posts/{postId}";
            var encodedUrl = Uri.EscapeUriString(rawUrl);
            return Accepted(encodedUrl, "درخواست لایک با موفقیت در صف قرار گرفت.");
        }
        
        [HttpPost("Create")]
        public async Task<IActionResult> CreatePost(CreatePostModel model)
        {
            var post = new Post(model.Title);
            await _appDbContext.Posts.AddAsync(post); // فقط اضافه به صف
            await _appDbContext.SaveChangesAsync();
            var rawUrl = $"/api/posts/{post.Id}";
            var encodedUrl = Uri.EscapeUriString(rawUrl);
            return Created(encodedUrl, "پست ایجاد شد.");
        }
    }
}
