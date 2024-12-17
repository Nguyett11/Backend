using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Controllers;
using WebProject.DataConnection;
using WebProject.Models;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebProject.Tests.Controllers
{
    public class Order_DetailsControllerTests
    {
        private readonly Order_DetailsController _controller;
        private readonly ApplicationDBContext _context;

        public Order_DetailsControllerTests()
        {
            // Sử dụng In-Memory Database cho môi trường test
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new ApplicationDBContext(options);

            // Khởi tạo controller với context
            _controller = new Order_DetailsController(_context);

            // Seed dữ liệu vào cơ sở dữ liệu
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            if (_context.Order_Details.Any())
                return;

            _context.Order_Details.AddRange(
                new Order_Details { id = 2, order_id = 1, product_id = 3, price = 29000000, number_of_products = 1, total_money = 29000000 },
                new Order_Details { id = 5, order_id = 1, product_id = 3, price = 30000000, number_of_products = 1, total_money = 30000000 }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetOrder_Details_ReturnsOkResultWithData()
        {
            // Act
            var result = await _controller.GetOrder_Details();

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orderDetails = Assert.IsType<List<Order_Details>>(okResult.Value);
            Assert.Equal(2, orderDetails.Count);
        }

        [Fact]
        public async Task GetOrder_DetailsById_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetOrderDetailsById(2);

            // Assert
            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var orderDetails = Assert.IsType<Order_Details>(okResult.Value);
            Assert.Equal(2, orderDetails.id);
        }


        [Fact]
        public async Task GetOrder_DetailsById_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetOrderDetailsById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderDetailsByOrderId_ReturnsOkResultWithSingleDetail()
        {
            // Act
            var result = await _controller.GetOrderDetailsByOrderId(1); // Giả sử có hai chi tiết đơn hàng

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // Kiểm tra kết quả Ok
            var orderDetailsList = Assert.IsType<List<Order_Details>>(okResult.Value); // Kiểm tra giá trị trả về là một List<Order_Details>
            Assert.Equal(2, orderDetailsList.Count); // Kiểm tra có 2 chi tiết trong danh sách
        }


        [Fact]
        public async Task GetOrderDetailsByOrderId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetOrderDetailsByOrderId(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostOrder_Details_ReturnsCreatedResult()
        {
            // Arrange
            var newOrderDetail = new Order_Details { id = 4, order_id = 1, product_id = 3, price = 30000000, number_of_products = 2, total_money = 60000000 };

            // Act
            var result = await _controller.PostOrder_Details(newOrderDetail);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var orderDetail = Assert.IsType<Order_Details>(createdResult.Value);
            Assert.Equal(4, orderDetail.id);
        }

        [Fact]
        public async Task PostOrder_Details_ReturnsBadRequestForMissingFields()
        {
            // Arrange
            var newOrderDetail = new Order_Details { product_id = 3, price = 30000000 }; // Missing required fields like order_id, number_of_products, total_money

            // Act
            var result = await _controller.PostOrder_Details(newOrderDetail);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostOrder_Details_ReturnsBadRequestForInvalidData()
        {
            // Arrange
            var newOrderDetail = new Order_Details { id = 6, order_id = 1, product_id = 3, price = -5000, number_of_products = 1, total_money = -5000 };

            // Act
            var result = await _controller.PostOrder_Details(newOrderDetail);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }



        [Fact]
        public async Task PutOrder_Details_ReturnsNoContentOnSuccess()
        {
            // Arrange: Lấy thực thể hiện có từ cơ sở dữ liệu
            var existingOrderDetail = await _context.Order_Details.FindAsync(2);  // Giả sử id = 2 là một chi tiết đơn hàng tồn tại
            Assert.NotNull(existingOrderDetail);

            // Cập nhật giá trị
            existingOrderDetail.price = 30000000;
            existingOrderDetail.number_of_products = 1;
            existingOrderDetail.total_money = 30000000;

            // Act: Gửi yêu cầu PUT với id = 2 và cập nhật giá trị
            var result = await _controller.PutOrder_Details(2, existingOrderDetail);  // id = 2 trong URL và body phải khớp

            // Assert: Kiểm tra kết quả trả về là NoContentResult
            Assert.IsType<NoContentResult>(result);
        }



        [Fact]
        public async Task PutOrder_Details_ReturnsBadRequestForInvalidId()
        {
            // Arrange
            var updatedOrderDetail = new Order_Details { id = 2, order_id = 1, product_id = 3, price = 30000000, number_of_products = 1, total_money = 30000000 };

            // Act
            var result = await _controller.PutOrder_Details(999, updatedOrderDetail);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_Details_ReturnsNoContent()
        {
            // Act
            var result = await _controller.DeleteOrder_Details(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_Details_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteOrder_Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}

