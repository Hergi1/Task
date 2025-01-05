// DTOs/CategoryCreateDTO.cs
namespace Task.DTOs
{
    public class CategoryCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

// DTOs/CategoryReadDTO.cs
namespace Task.DTOs
{
    public class CategoryReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
