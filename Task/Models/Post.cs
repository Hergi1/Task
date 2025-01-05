namespace Task.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; } 
        public ICollection<Category> Categories { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
