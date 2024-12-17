using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Controllers;
using WebProject.DataConnection;
using WebProject.Models;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;

namespace WebProject.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly CategoriesController _controller;
        private readonly ApplicationDBContext _context;

        public CategoriesControllerTests()
        {
            // Sử dụng In-Memory Database cho môi trường test
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase("TestDatabase") // Đặt tên cơ sở dữ liệu In-Memory
                .Options;


            _context = new ApplicationDBContext(options);

            // Khởi tạo controller với context đã được tạo
            _controller = new CategoriesController(_context);

            // Seed dữ liệu vào cơ sở dữ liệu
            SeedDatabase();
        }

        // Seed dữ liệu vào cơ sở dữ liệu In-Memory
        private void SeedDatabase()
        {
            if (_context.Categories.Any())
            {
                return; // Nếu đã có dữ liệu, không thêm lại
            }

            _context.Categories.Add(new Categories { category_id = 1, category_name = "Smartphones" });
            _context.Categories.Add(new Categories { category_id = 2, category_name = "Laptop" });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetCategories_ReturnsOkResultWithCorrectData()
        {
            // Act: Gọi phương thức GetCategories từ controller
            var result = await _controller.GetCategories();

            // Assert: Kiểm tra kết quả trả về không phải null
            Assert.NotNull(result);

            // Kiểm tra kiểu trả về là OkObjectResult
           var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // Kiểm tra dữ liệu trả về là danh sách Categories
            var categories = Assert.IsType<List<Categories>>(okResult.Value);

            // Kiểm tra số lượng danh mục
            Assert.Equal(2, categories.Count); // Kiểm tra số lượng danh mục
            Assert.Contains(categories, c => c.category_name == "Smartphones"); // Kiểm tra "Electronics" có trong danh sách
            Assert.Contains(categories, c => c.category_name == "Laptop"); // Kiểm tra "Books" có trong danh sách
        }

        [Fact]
        public async Task GetCategories_ReturnsNotFoundWhenNoCategoriesExist()
        {
            // Arrange: Clear the categories from the database to simulate no data
            _context.Categories.RemoveRange(_context.Categories);
            await _context.SaveChangesAsync();

            // Act: Attempt to get categories
            var result = await _controller.GetCategories();

            // Assert: Ensure the result is NotFound
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetCategoriesById_ReturnsOkResultWithCorrectData()
        {
            // Arrange
            var categoryId = 1; // Giả sử có một category với id = 1 trong cơ sở dữ liệu

            // Act
            var result = await _controller.GetCategories(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // Kiểm tra kết quả trả về là OkObjectResult
            var category = Assert.IsType<Categories>(okResult.Value); // Kiểm tra giá trị trả về là đối tượng Categories
            Assert.Equal(categoryId, category.category_id); // Kiểm tra id của category
        }


        [Fact]
        public async Task GetCategoriesById_ReturnsNotFoundWhenCategoryDoesNotExist()
        {
            // Act: Gọi phương thức GetCategories với id không hợp lệ
            var result = await _controller.GetCategories(999);

            // Assert: Kiểm tra kết quả trả về là NotFoundResult
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostCategories_ReturnsCreatedResult()
        {
            // Arrange: Tạo một danh mục mới
            var newCategory = new Categories { category_id = 3, category_name = "Tablets" };

            // Act: Gọi phương thức PostCategories
            var result = await _controller.PostCategories(newCategory);

            // Assert: Kiểm tra kết quả trả về là CreatedAtActionResult
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            // Kiểm tra dữ liệu trả về đúng
            var createdCategory = Assert.IsType<Categories>(createdResult.Value);
            Assert.Equal(newCategory.category_id, createdCategory.category_id);
            Assert.Equal(newCategory.category_name, createdCategory.category_name);
        }


        [Fact]
        public async Task PostCategories_ReturnsCreatedForValidInput()
        {
            // Arrange
            var newCategory = new Categories { category_id = 4, category_name = "accessories" };

            // Act
            var result = await _controller.PostCategories(newCategory);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdBrand = Assert.IsType<Categories>(createdResult.Value);
            Assert.Equal("accessories", createdBrand.category_name);
        }

        [Fact]
        public async Task PutCategories_ReturnsNoContentOnSuccess()
        {
    
            // Arrange: Lấy thực thể hiện có từ cơ sở dữ liệu
            var existingCategories = await _context.Categories.FindAsync(1);
            Assert.NotNull(existingCategories);

            // Cập nhật giá trị
            existingCategories.category_name = "Smart Devices";
            // Act: Gọi phương thức PutCategories
            var result = await _controller.PutCategories(1, existingCategories);

            // Assert: Kiểm tra kết quả trả về là NoContent
            Assert.IsType<NoContentResult>(result);

            // Kiểm tra cơ sở dữ liệu đã được cập nhật
            var categoryInDb = await _context.Categories.FindAsync(1);
            Assert.Equal("Smart Devices", categoryInDb.category_name);
            // Assert
            //Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task PutCategories_ReturnsNotFoundWhenCategoryDoesNotExist()
        {
            // Arrange: Tạo một danh mục mới với ID không tồn tại trong cơ sở dữ liệu
            var updatedCategory = new Categories { category_id = 999, category_name = "Nonexistent Category" };

            // Act: Gọi phương thức PutCategories với ID không tồn tại
            var result = await _controller.PutCategories(999, updatedCategory);

            // Assert: Kiểm tra kết quả trả về là NotFound
            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task PutCategories_ReturnsBadRequestWhenIdMismatch()
        {
            // Arrange: Tạo một danh mục với id không khớp
            var updatedCategory = new Categories { category_id = 2, category_name = "Updated Laptop" };

            // Act: Gọi phương thức PutCategories
            var result = await _controller.PutCategories(1, updatedCategory);

            // Assert: Kiểm tra kết quả trả về là BadRequest
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteCategories_ReturnsNoContentOnSuccess()
        {
            // Act: Gọi phương thức DeleteCategories với id hợp lệ
            var result = await _controller.DeleteCategories(1);

            // Assert: Kiểm tra kết quả trả về là NoContent
            Assert.IsType<NoContentResult>(result);

            // Kiểm tra danh mục đã bị xóa khỏi cơ sở dữ liệu
            var categoryInDb = await _context.Categories.FindAsync(1);
            Assert.Null(categoryInDb);
        }

        [Fact]
        public async Task DeleteCategories_ReturnsNotFoundWhenCategoryDoesNotExist()
        {
            // Act: Gọi phương thức DeleteCategories với id không hợp lệ
            var result = await _controller.DeleteCategories(999);

            // Assert: Kiểm tra kết quả trả về là NotFound
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
