namespace BatchProcessor.Dtos
{
    public class PostLikeDto
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
