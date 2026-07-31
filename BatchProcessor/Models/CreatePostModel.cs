using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BatchProcessor.Models
{
    public class CreatePostModel
    {
        public string Title { get; set; }

        public CreatePostModel(string title)
        {
            Title = title;
        }
    }
}
