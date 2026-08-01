using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ConstrainedExecution;

namespace BatchProcessor.Entities
{
    public class PostLike
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
