// Controllers/PostController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task.Data;
using Task.Models;
using Task.DTOs;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new post.
        /// </summary>
        /// <param name="postDto">Post creation details.</param>
        /// <returns>Created post details.</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateDTO postDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract UserId from JWT Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { Message = "Invalid token." });

            // Validate User
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { Message = "User not found." });

            // Validate Categories
            var categories = await _context.Categories
                .Where(c => postDto.CategoryIds.Contains(c.Id))
                .ToListAsync();

            if (categories.Count != postDto.CategoryIds.Count)
                return BadRequest(new { Message = "One or more categories not found." });

            var post = new Post
            {
                Title = postDto.Title,
                Content = postDto.Content,
                Status = postDto.Status,
                CreationDate = DateTime.UtcNow, // Set to current UTC time
                PublishDate = postDto.PublishDate,
                UserId = userId,
                Categories = categories
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var postReadDto = new PostReadDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Status = post.Status,
                CreationDate = post.CreationDate,
                PublishDate = post.PublishDate,
                UserId = post.UserId,
                Username = user.Username,
                Role = user.Role,
                Categories = categories.Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postReadDto);
        }

        /// <summary>
        /// Retrieves a post by its ID.
        /// </summary>
        /// <param name="id">Post ID.</param>
        /// <returns>Post details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Categories)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound(new { Message = "Post not found." });

            var postReadDto = new PostReadDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Status = post.Status,
                CreationDate = post.CreationDate,
                PublishDate = post.PublishDate,
                UserId = post.UserId,
                Username = post.User.Username,
                Role = post.User.Role,
                Categories = post.Categories.Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };

            return Ok(postReadDto);
        }

        /// <summary>
        /// Retrieves all posts with optional search and date filters.
        /// </summary>
        /// <param name="searchText">Search text to filter posts by title or content.</param>
        /// <param name="publishDate">Publish date to filter posts.</param>
        /// <returns>List of filtered posts.</returns>
        [HttpGet("GetAllPosts")]
        public async Task<IActionResult> GetAllPosts([FromQuery] string searchText, [FromQuery] DateTime? publishDate)
        {
            // Initialize the query with related entities
            var query = _context.Posts
                .Include(p => p.Categories)
                .Include(p => p.User)
                .AsQueryable();

            // Apply search filter if searchText is provided
            if (!string.IsNullOrEmpty(searchText))
            {
                var loweredSearchText = searchText.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(loweredSearchText) ||
                    p.Content.ToLower().Contains(loweredSearchText));
            }

            // Apply publish date filter if publishDate is provided
            if (publishDate.HasValue)
            {
                var date = publishDate.Value.Date;
                query = query.Where(p =>
                    p.PublishDate.HasValue &&
                    p.PublishDate.Value.Date == date);
            }

            // Execute the query and retrieve the list of posts
            var posts = await query
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();

            // Map the posts to PostReadDTO
            var postReadDtos = posts.Select(p => new PostReadDTO
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Status = p.Status,
                CreationDate = p.CreationDate,
                PublishDate = p.PublishDate,
                UserId = p.UserId,
                Username = p.User.Username,
                Role = p.User.Role,
                Categories = p.Categories.Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            }).ToList();

            // Prepare the response object
            var response = new
            {
                Posts = postReadDtos
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates an existing post.
        /// </summary>
        /// <param name="id">Post ID.</param>
        /// <param name="postDto">Post update details.</param>
        /// <returns>No content upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] PostCreateDTO postDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var post = await _context.Posts
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound(new { Message = "Post not found." });

            // Extract UserId from JWT Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { Message = "Invalid token." });

            // Ensure the authenticated user is the creator of the post
            if (post.UserId != userId)
                return StatusCode(403, new { Message = "You are not authorized to edit this post." });

            // Validate Categories
            var categories = await _context.Categories
                .Where(c => postDto.CategoryIds.Contains(c.Id))
                .ToListAsync();

            if (categories.Count != postDto.CategoryIds.Count)
                return BadRequest(new { Message = "One or more categories not found." });

            // Update post properties
            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.Status = postDto.Status;
            post.PublishDate = postDto.PublishDate;
            // Optionally, prevent updating CreationDate
            // post.CreationDate = postDto.CreationDate;
            post.Categories = categories;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes an existing post.
        /// </summary>
        /// <param name="id">Post ID.</param>
        /// <returns>No content upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { Message = "Post not found." });

            // Extract UserId from JWT Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { Message = "Invalid token." });

            // Ensure the authenticated user is the creator of the post
            if (post.UserId != userId)
                return StatusCode(403, new { Message = "You are not authorized to delete this post." });

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
