namespace Task.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; } // E.g., "Published", "Draft", "Deleted"
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; } // Nullable, in case the post isn't published yet

        // Foreign key for Category (many-to-many relation)
        public ICollection<Category> Categories { get; set; }

        // User that created the post (Navigation property)
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
