using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.DataConnection;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UsersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUserId(int id)
        {
            var userid = await _context.Users.FindAsync(id);

            if (userid == null)
            {
                return NotFound();
            }

            return userid;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.user_id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            // Gán giá trị mặc định cho role_id nếu không được cung cấp
            if (users.role_id == 0)  // Nếu role_id chưa được gán (mặc định là 0)
            {
                users.role_id = 2;  // Gán role_id mặc định là 2
            }

            _context.Users.Add(users);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsers", new { id = users.user_id }, users);
        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<ActionResult> DangNhap([FromForm] string username, [FromForm] string password)
        {
            // Log thông tin nhận được
            Console.WriteLine($"Username: {username}, Password: {password}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.username == username);

            if (user == null || !VerifyPassword(user, password))
            {
                return Unauthorized(new { success = false, message = "Đăng nhập thất bại" });
            }

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetString("UserId", user.user_id.ToString());
            HttpContext.Session.SetString("Username", user.username);
            HttpContext.Session.SetInt32("RoleId", user.role_id);

            // Kiểm tra quyền (role) của người dùng
            if (user.role_id == 1)
            {
                return Ok(new { success = true, message = "Đăng nhập thành công - Admin", role = "Admin", user = new { user.user_id, user.username, user.role_id } });
            }
            else if (user.role_id == 2)
            {
                return Ok(new { success = true, message = "Đăng nhập thành công - User", role = "User", user = new { user.user_id, user.username, user.role_id } });
            }
            else
            {
                return BadRequest(new { success = false, message = "Lỗi: role_id không hợp lệ" });
            }
        }


        private bool VerifyPassword(Users user, string password)
        {
            // Thực hiện logic xác thực mật khẩu tại đây
            return user.password == password;
        }


        // Đăng xuất
        [HttpPost("logout")]
        public IActionResult DangXuat()
        {
            // Xóa thông tin session
            HttpContext.Session.Clear();
            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }

        // PATCH: api/Users/{id}/password
        [HttpPatch("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("Invalid request data.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.password != request.CurrentPassword)
            {
                return Unauthorized("Current password is incorrect.");
            }

            // Cập nhật mật khẩu mới
            user.password = request.NewPassword;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Trả về NoContent nếu thành công
        }


        // Model để nhận dữ liệu từ yêu cầu
        public class UpdatePasswordRequest
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.user_id == id);
        }
    }
}
