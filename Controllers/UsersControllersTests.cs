using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProject.Controllers;
using WebProject.DataConnection;
using WebProject.Models;
using Xunit;

namespace WebProject.Tests.Controllers
{
    public class UsersControllersTests
    {
        private readonly UsersController _controller;
        private readonly ApplicationDBContext _context;

        public UsersControllersTests()
        {
            // Set up In-Memory Database for testing
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase("UsersTestDatabase") // Set the in-memory database name
                .Options;

            _context = new ApplicationDBContext(options);

            // Initialize the controller with the created context
            _controller = new UsersController(_context);

            // Seed data into the database
            SeedDatabase();
        }

        // Seed data into the in-memory database
        private void SeedDatabase()
        {
            if (_context.Users.Any())
            {
                return; // If there is data, do not add again
            }

            // Seed with actual expected data
            _context.Users.Add(new Users
            {
                user_id = 3,
                username = "Nguyen Van A",
                email = "nguyenvana@example.com",
                phone = "1234567890",
                password = "123",  // Ensure password is not NULL
                address = "123 Admin St.",
                create_at = new DateTime(2024, 12, 12), // Use specific date if needed
                role_id = 1
            });

            _context.Users.Add(new Users
            {
                user_id = 4,
                username = "Nguyen Van B",
                email = "john@example.com",
                phone = "9876543210",
                password = "password123", // Assign a non-null password for testing
                address = "Tp.Quy Nhon",
                create_at = new DateTime(2024, 12, 12), // Use specific date if needed
                role_id = 2
            });

            // Save changes to persist the data
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUserId_ReturnsOkResultWithCorrectData()
        {
            // Act
            var result = await _controller.GetUserId(3);  // Fetching user with user_id = 3

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);  // Ensure the result is OkObjectResult
            var user = Assert.IsType<Users>(okResult.Value);  // Verify that the result is of type Users
            Assert.Equal(3, user.user_id);  // Ensure the correct user ID
            Assert.Equal("Nguyen Van A", user.username);  // Verify the username
        }


        [Fact]
        public async Task GetUserId_ReturnsNotFoundWhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.GetUserId(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUsersWithRole2_ReturnsOkResultWithCorrectData()
        {
            // Act
            var result = await _controller.GetUsersWithRole2();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsType<List<Users>>(okResult.Value);
            Assert.Single(users); // Should contain only the user with role_id 2
            Assert.Contains(users, u => u.username == "Nguyen Van B");
        }

        [Fact]
        public async Task PostUsers_ReturnsCreatedResult()
        {
            // Arrange
            var newUser = new Users { username = "newUser", password = "newUser123", role_id = 2 };

            // Act
            var result = await _controller.PostUsers(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdUser = Assert.IsType<Users>(createdResult.Value);
            Assert.Equal(newUser.username, createdUser.username);
        }

        [Fact]
        public async Task PutUsers_ReturnsNoContentOnSuccess()
        {
            // Arrange
            var existingUser = await _context.Users.FindAsync(2);
            Assert.NotNull(existingUser);

            existingUser.username = "updatedUser";

            // Act
            var result = await _controller.PutUsers(2, existingUser);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedUser = await _context.Users.FindAsync(2);
            Assert.Equal("updatedUser", updatedUser.username);
        }

        [Fact]
        public async Task PutUsers_ReturnsBadRequestWhenIdMismatch()
        {
            // Arrange
            var updatedUser = new Users { user_id = 2, username = "Updated User" };

            // Act
            var result = await _controller.PutUsers(1, updatedUser);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContentOnSuccess()
        {
            // Act
            var result = await _controller.DeleteUser(2);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var userInDb = await _context.Users.FindAsync(2);
            Assert.Null(userInDb);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFoundWhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteUser(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DangNhap_ReturnsOkForValidCredentials()
        {
            // Arrange
            var validUsername = "Nguyen Van A";  // Replace with a valid username from your DB
            var validPassword = "123";  // Replace with the correct password for that username

            // Mock ISession
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>())).Verifiable();

            // Set up the controller context with the mocked session
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = sessionMock.Object
                }
            };

            // Ensure the database is seeded with the test data
            if (!_context.Users.Any())
            {
                _context.Users.Add(new Users
                {
                    user_id = 1,
                    username = "Nguyen Van A",
                    password = "123",  // Password must match
                    role_id = 1
                });
                _context.SaveChanges();
            }

            // Act
            var result = await _controller.DangNhap(validUsername, validPassword);

            // Assert: Check if the response is Ok and contains the correct user data
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Cast the value to the anonymous type you expect
            var value = okResult.Value as dynamic;

            // Access the properties of the anonymous object correctly
            Assert.NotNull(value);  // Ensure that the value is not null
            Assert.True(value.success);  // Ensure success is true
            Assert.Equal("Đăng nhập thành công - Admin", value.message);  // Ensure correct message
            Assert.Equal("Admin", value.role);  // Ensure correct role is returned
            Assert.Equal(1, value.user.user_id);  // Ensure user_id is correct
            Assert.Equal("Nguyen Van A", value.user.username);  // Ensure correct username
            Assert.Equal(1, value.user.role_id);  // Ensure correct role_id (Admin role in this case)

            // Verify session methods were called correctly
            sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }









        [Fact]
        public async Task DangNhap_ReturnsUnauthorizedForInvalidCredentials()
        {
            // Act
            var result = await _controller.DangNhap("admin", "wrongPassword");

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_ReturnsNoContentOnSuccess()
        {
            // Arrange
            var userId = 3;
            var updateRequest = new UsersController.UpdatePasswordRequest
            {
                CurrentPassword = "123",  // Mật khẩu cũ chính xác
                NewPassword = "new123"    // Mật khẩu mới
            };

            // Kiểm tra người dùng tồn tại trong DB
            var user = await _context.Users.FindAsync(userId);
            Assert.NotNull(user);

            // Act: Cập nhật mật khẩu
            var result = await _controller.UpdatePassword(userId, updateRequest);

            // Assert: Kiểm tra phản hồi trả về NoContent (cập nhật thành công)
            Assert.IsType<NoContentResult>(result);

            // Kiểm tra mật khẩu mới đã được cập nhật trong DB
            var updatedUser = await _context.Users.FindAsync(userId);
            Assert.Equal(updateRequest.NewPassword, updatedUser.password);
        }

        [Fact]
        public async Task UpdatePassword_ReturnsUnauthorizedForIncorrectCurrentPassword()
        {
            // Arrange
            var updateRequest = new UsersController.UpdatePasswordRequest
            {
                CurrentPassword = "wrongPassword",
                NewPassword = "newUser123"
            };

            // Act
            var result = await _controller.UpdatePassword(2, updateRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
