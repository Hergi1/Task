// DTOs/PostCreateDTO.cs
using System;


namespace Task.DTOs
{
    public class PostCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; } // e.g., "Published", "Draft"
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; } // Nullable
        public List<int> CategoryIds { get; set; } // List of Category IDs
        public int UserId { get; set; }
    }
}

// DTOs/PostReadDTO.cs
namespace Task.DTOs
{
    public class PostReadDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public List<CategoryReadDTO> Categories { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
