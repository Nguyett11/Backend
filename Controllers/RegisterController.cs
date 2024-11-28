using Microsoft.AspNetCore.Mvc;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private static List<RegisterRequest> users = new List<RegisterRequest>();
        private static int nextId = 1;

        // GET: api/Register
        [HttpGet]
        public ActionResult<IEnumerable<RegisterRequest>> GetUsers()
        {
            return Ok(users);
        }

        // GET: api/Register/{id}
        [HttpGet("{id}")]
        public ActionResult<RegisterRequest> GetUserById(int id)
        {
            var user = users.FirstOrDefault(u => u.user_id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        // POST: api/Register
        [HttpPost]
        public ActionResult<RegisterRequest> CreateUser([FromBody] RegisterRequest newUser)
        {
            if (newUser == null)
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = users.FirstOrDefault(u => u.email == newUser.email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email already exists." });
            }
            //// Mặc định role_id là 2 nếu không có giá trị
            //if (newUser.role_id == 0)
            //{
            //    newUser.role_id = 2; // Gán giá trị mặc định là 2
            //}
            newUser.user_id = nextId++;
            newUser.create_at = DateTime.Now;
            users.Add(newUser);

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.user_id }, newUser);
        }


        // PUT: api/Register/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, [FromBody] RegisterRequest updatedUser)
        {
            var user = users.FirstOrDefault(u => u.user_id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.username = updatedUser.username;
            user.email = updatedUser.email;
            user.phone = updatedUser.phone;
            user.password = updatedUser.password;
            user.address = updatedUser.address;
            user.role_id = updatedUser.role_id;

            return NoContent();
        }

        // DELETE: api/Register/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            var user = users.FirstOrDefault(u => u.user_id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            users.Remove(user);
            return NoContent();
        }
    }
}
