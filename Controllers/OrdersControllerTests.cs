using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProject.Controllers;
using WebProject.DataConnection;
using WebProject.Models;
using Xunit;
using static WebProject.Controllers.OrdersController;

namespace WebProject.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly OrdersController _controller;
        private readonly ApplicationDBContext _context;

        public OrdersControllerTests()
        {
            // Sử dụng In-Memory Database cho môi trường test
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);

            // Khởi tạo controller với context
            _controller = new OrdersController(_context);

            // Seed dữ liệu vào cơ sở dữ liệu
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            if (_context.Orders.Any())
                return;

            _context.Orders.AddRange(
                new Orders { order_id = 1, customer_id = 2, order_status = "Đang xử lý", create_at = System.DateTime.Now, total_amount = 31000001 },
                new Orders { order_id = 2, customer_id = 4, order_status = "Đã hoàn tất", create_at = System.DateTime.Now.AddDays(-1), total_amount = 40000000 }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResultWithData()
        {
            // Act
            var result = await _controller.GetOrders();

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orders = Assert.IsType<List<Orders>>(okResult.Value);
            Assert.Equal(2, orders.Count);
        }

        [Fact]
        public async Task GetOrders_ReturnsNotFoundWhenNoData()
        {
            // Arrange: Clear the database to simulate no data
            _context.Orders.RemoveRange(_context.Orders);
            await _context.SaveChangesAsync();

            // Act: Attempt to get orders
            var result = await _controller.GetOrders();

            // Assert: Ensure the result is NotFound
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetOrdersById_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetOrders(1);

            // Assert
            Assert.NotNull(result);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var order = Assert.IsType<Orders>(okResult.Value);
            Assert.Equal(1, order.order_id);
        }


        [Fact]
        public async Task GetOrdersById_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetOrders(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostOrders_ReturnsCreatedResult()
        {
            // Arrange
            var newOrder = new Orders { order_id = 3, customer_id = 2, order_status = "Đang xử lý", create_at = System.DateTime.Now, total_amount = 75000000 };

            // Act
            var result = await _controller.PostOrders(newOrder);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var order = Assert.IsType<Orders>(createdResult.Value);
            Assert.Equal(3, order.order_id);
        }




        [Fact]
        public async Task PostOrders_ReturnsCreatedForValidInput()
        {
            // Arrange
            var newOrder = new Orders { order_id = 3, customer_id = 2, order_status = "Đang xử lý", create_at = System.DateTime.Now, total_amount = 75000000 };

            // Act
            var result = await _controller.PostOrders(newOrder);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdOrder = Assert.IsType<Orders>(createdResult.Value);

            Assert.Equal(newOrder.order_id, createdOrder.order_id);
            Assert.Equal(newOrder.customer_id, createdOrder.customer_id);
            Assert.Equal(newOrder.order_status, createdOrder.order_status);
            Assert.Equal(newOrder.total_amount, createdOrder.total_amount);
        }


        [Fact]
        public async Task PutOrders_ReturnsNoContentOnSuccess()
        {
            // Arrange: Lấy thực thể hiện có từ cơ sở dữ liệu
            var existingOrder = await _context.Orders.FindAsync(1);
            Assert.NotNull(existingOrder);

            // Cập nhật giá trị
            existingOrder.order_status = "Đã hoàn tất";
            existingOrder.total_amount = 150.0m;

            // Act
            var response = await _controller.PutOrders(1, existingOrder);

            // Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task PutOrders_ReturnsNotFoundWhenOrderDoesNotExist()
        {
            // Arrange: Create an order object with a non-existing ID (e.g., 999)
            var nonExistingOrder = new Orders
            {
                order_id = 999,  // ID that does not exist in the database
                customer_id = 1,
                order_status = "Đang xử lý",
                create_at = System.DateTime.Now,
                total_amount = 20000000
            };

            // Act: Attempt to update an order that does not exist
            var result = await _controller.PutOrders(999, nonExistingOrder);

            // Assert: Ensure the result is NotFound
            Assert.IsType<NotFoundResult>(result);
        }




        [Fact]
        public async Task DeleteOrderWithDetails_ReturnsNoContent()
        {
            // Act
            var result = await _controller.DeleteOrderWithDetails(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }


        [Fact]
        public async Task DeleteOrder_ReturnsNotFoundWhenCategoryDoesNotExist()
        {
            // Act: Gọi phương thức DeleteCategories với id không hợp lệ
            var result = await _controller.DeleteOrderWithDetails(999);

            // Assert: Kiểm tra kết quả trả về là NotFound
            Assert.IsType<NotFoundResult>(result);
        }



        [Fact]
        public async Task GetTimKiem_ReturnsOkResult()
        {
            // Arrange: Dữ liệu kiểm tra phải có trong cơ sở dữ liệu
            var searchTerm = "1"; // Kiểm tra với giá trị khớp trong cơ sở dữ liệu

            // Act: Thực thi hành động
            var result = await _controller.GetTimKiem(searchTerm);

            // Assert: Kiểm tra kết quả
            var okResult = Assert.IsType<OkObjectResult>(result.Result); // Kiểm tra trả về OkObjectResult
            var orders = Assert.IsType<List<Orders>>(okResult.Value); // Kiểm tra kiểu dữ liệu trả về
            Assert.NotEmpty(orders); // Kiểm tra danh sách đơn hàng không rỗng
        }

        [Fact]
        public async Task GetTimKiem_ReturnsNotFoundWhenNoResults()
        {
            // Arrange: Dữ liệu kiểm tra không có trong cơ sở dữ liệu
            var searchTerm = "999"; // Kiểm tra với giá trị không có trong cơ sở dữ liệu

            // Act: Thực thi hành động
            var result = await _controller.GetTimKiem(searchTerm);

            // Assert: Kiểm tra trả về NotFoundResult
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTimKiem_ReturnsBadRequestWhenSearchTermIsInvalid()
        {
            // Arrange: Test with an empty search term (or null)
            var searchTerm = string.Empty;  // Empty search term, which should be invalid

            // Act: Execute the action with the invalid search term
            var result = await _controller.GetTimKiem(searchTerm);

            // Assert: Ensure the result is BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Văn bản tìm kiếm không được để trống.", badRequestResult.Value);  // Update the expected message to match the actual message
        }



        [Fact]
        public async Task PatchOrderStatus_ReturnsNoContentOnSuccess()
        {
            // Arrange
            var orderId = 1;
            var newStatus = "Completed";
            var orderStatusUpdate = new OrderStatusUpdate { OrderStatus = newStatus };

            // Ensure the order exists in the database
            var existingOrder = await _context.Orders.FindAsync(orderId);
            if (existingOrder == null)
            {
                existingOrder = new Orders
                {
                    order_id = orderId,
                    customer_id = 1,
                    order_status = "Pending",
                    create_at = DateTime.Now,
                    total_amount = 50000
                };

                _context.Orders.Add(existingOrder);
                await _context.SaveChangesAsync();
            }

            // Act
            var result = await _controller.PatchOrderStatus(orderId, orderStatusUpdate);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the update in the database
            var updatedOrder = await _context.Orders.FindAsync(orderId);
            Assert.NotNull(updatedOrder);
            Assert.Equal(newStatus, updatedOrder.order_status);
        }

        [Fact]
        public async Task PatchOrderStatus_ReturnsBadRequestWhenOrderStatusIsEmpty()
        {
            // Arrange
            var orderStatusUpdate = new OrderStatusUpdate { OrderStatus = string.Empty };

            // Act
            var result = await _controller.PatchOrderStatus(1, orderStatusUpdate);

            // Assert`
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Order status cannot be empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task PatchOrderStatus_ReturnsNotFoundWhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 999; // Assume this order does not exist in the database
            var newStatus = "Shipped";
            var orderStatusUpdate = new OrderStatusUpdate { OrderStatus = newStatus };

            // Act
            var result = await _controller.PatchOrderStatus(orderId, orderStatusUpdate);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);  // Expect NotFoundResult
        }



    }
}
