using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProject.Controllers;
using WebProject.DataConnection;
using WebProject.Models;
using Xunit;

namespace WebProject.Tests.Controllers
{
    public class BrandsControllerTests
    {
        private readonly BrandsController _controller;
        private readonly ApplicationDBContext _context;

        public BrandsControllerTests()
        {
            // Sử dụng In-Memory Database cho môi trường test
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase("BrandsTestDatabase") // Đặt tên cơ sở dữ liệu In-Memory
                .Options;

            _context = new ApplicationDBContext(options);

            // Khởi tạo controller với context đã được tạo
            _controller = new BrandsController(_context);

            // Seed dữ liệu vào cơ sở dữ liệu
            SeedDatabase();
        }

        // Seed dữ liệu vào cơ sở dữ liệu In-Memory
        private void SeedDatabase()
        {
            if (_context.Brands.Any())
            {
                return; // Nếu đã có dữ liệu, không thêm lại
            }

            _context.Brands.Add(new Brands { brand_id = 2, brand_name = "Apple", category_id = 1 });
            _context.Brands.Add(new Brands { brand_id = 3, brand_name = "Samsung", category_id = 1 });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetBrands_ReturnsOkResultWithCorrectData()
        {
            // Act
            var result = await _controller.GetBrands();

            // Assert
            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var brands = Assert.IsType<List<Brands>>(okResult.Value);

            Assert.Equal(2, brands.Count);
            Assert.Contains(brands, b => b.brand_name == "Apple");
            Assert.Contains(brands, b => b.brand_name == "Samsung");
        }

        [Fact]
        public async Task GetBrands_ReturnsNotFoundWhenNoData()
        {
            // Arrange: Remove all existing brands from the database
            _context.Brands.RemoveRange(_context.Brands);
            await _context.SaveChangesAsync();

            // Act: Attempt to get brands when the database is empty
            var result = await _controller.GetBrands();

            // Assert: Ensure the result is NotFound
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetBrandsById_ReturnsOkResultWithCorrectData()
        {

            // Act
            var result = await _controller.GetBrands(2);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var brand = Assert.IsType<Brands>(okResult.Value);
            Assert.Equal(2, brand.brand_id);
            Assert.Equal("Apple", brand.brand_name);
        }

        //Không tìm thấy khi thương hiệu không tồn tại
        [Fact]
        public async Task GetBrandsById_ReturnsNotFoundWhenBrandDoesNotExist()
        {
            // Act
            var result = await _controller.GetBrands(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        //Trả về không tìm thấy cho Id không hợp lệ
        [Fact]
        public async Task GetBrandsById_ReturnsNotFoundForInvalidId()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var result = await _controller.GetBrands(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetBrandsByCategory_ReturnsCorrectData()
        {
            // Act
            var result = await _controller.GetBrandsByCategory(1);

            // Assert
            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var brands = Assert.IsType<List<Brands>>(okResult.Value); // Cast to List<Brands>
            Assert.NotNull(brands);
            Assert.Equal(2, brands.Count);
            Assert.Contains(brands, b => b.brand_name == "Apple");
            Assert.Contains(brands, b => b.brand_name == "Samsung");
        }


        [Fact]
        public async Task GetBrandsByCategory_ReturnsNotFoundWhenCategoryHasNoBrands()
        {
            // Act
            var result = await _controller.GetBrandsByCategory(999); // Category ID không tồn tại

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }


        [Fact]
        public async Task PostBrands_ReturnsCreatedResult()
        {
            // Arrange
            var newBrand = new Brands { brand_id = 4, brand_name = "Sony", category_id = 2 };

            // Act
            var result = await _controller.PostBrands(newBrand);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdBrand = Assert.IsType<Brands>(createdResult.Value);
            Assert.Equal(newBrand.brand_name, createdBrand.brand_name);
        }

        [Fact]
        public async Task PostBrands_ReturnsCreatedForValidInput()
        {
            // Arrange
            var validBrand = new Brands { brand_id = 6, brand_name = "Huawei", category_id = 1 };

            // Act
            var result = await _controller.PostBrands(validBrand);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdBrand = Assert.IsType<Brands>(createdResult.Value);
            Assert.Equal("Huawei", createdBrand.brand_name);
        }

        [Fact]
        public async Task PutBrands_ReturnsNoContentOnSuccess()
        {
            // Arrange
            var existingBrand = await _context.Brands.FindAsync(2);
            Assert.NotNull(existingBrand);

            existingBrand.brand_name = "Apple Inc.";

            // Act
            var result = await _controller.PutBrands(2, existingBrand);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedBrand = await _context.Brands.FindAsync(2);
            Assert.Equal("Apple Inc.", updatedBrand.brand_name);
        }

        [Fact]
        public async Task PutBrands_ReturnsBadRequestWhenIdMismatch()
        {
            // Arrange
            var updatedBrand = new Brands { brand_id = 2, brand_name = "Updated Brand" };

            // Act
            var result = await _controller.PutBrands(1, updatedBrand);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteBrands_ReturnsNoContentOnSuccess()
        {
            // Act
            var result = await _controller.DeleteBrands(2);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var brandInDb = await _context.Brands.FindAsync(2);
            Assert.Null(brandInDb);
        }

        [Fact]
        public async Task DeleteBrands_ReturnsNotFoundWhenBrandDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteBrands(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
