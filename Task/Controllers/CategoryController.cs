// Controllers/CategoryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task.Data;
using Task.Models;
using Task.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDto">Category creation details.</param>
        /// <returns>Created category details.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower()))
                return BadRequest(new { Message = "Category already exists." });

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                Posts = new List<Post>() // Initialize to prevent null reference
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryReadDto = new CategoryReadDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, categoryReadDto);
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <returns>Category details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Message = "Category not found." });

            var categoryReadDto = new CategoryReadDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return Ok(categoryReadDto);
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="categoryDto">Category update details.</param>
        /// <returns>No content upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryCreateDTO categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Message = "Category not found." });

            if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower() && c.Id != id))
                return BadRequest(new { Message = "Another category with the same name already exists." });

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes an existing category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <returns>No content upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { Message = "Category not found." });

            if (category.Posts.Any())
                return BadRequest(new { Message = "Cannot delete category linked to posts." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
