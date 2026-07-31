using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatchProcessor.Entities
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PostLike> PostLikes { get; set; }

        public Post(string title)
        {
            CreatedAt = DateTime.Now;
            Id = Guid.NewGuid();

            Title = title;
        }

    }
}
