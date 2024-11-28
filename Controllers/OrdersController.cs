using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.DataConnection;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public OrdersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orders>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Orders>> GetOrders(int id)
        {
            var orders = await _context.Orders.FindAsync(id);

            if (orders == null)
            {
                return NotFound();
            }

            return orders;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrders(int id, Orders orders)
        {
            if (id != orders.order_id)
            {
                return BadRequest();
            }

            _context.Entry(orders).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrdersExists(id))
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Orders>> PostOrders(Orders orders)
        {
            _context.Orders.Add(orders);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OrdersExists(orders.order_id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrders", new { id = orders.order_id }, orders);
        }



        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrders(string id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("deleteorders/{orderId}")]
        public async Task<IActionResult> DeleteOrderWithDetails(int orderId)
        {
            // Tìm tất cả các bản ghi trong Order_Details liên kết với orderId
            var orderDetails = _context.Order_Details.Where(od => od.order_id == orderId);

            // Xóa các bản ghi trong Order_Details liên quan
            _context.Order_Details.RemoveRange(orderDetails);

            // Tìm bản ghi trong Orders theo orderId
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy đơn hàng
            }

            // Xóa bản ghi trong Orders
            _context.Orders.Remove(order);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return NoContent(); // Trả về 204 No Content khi xóa thành công
        }

        // GET: api/Orders/customer/{customerId}
        //[HttpGet("customer/{customerId}")]
        //public async Task<ActionResult<IEnumerable<Orders>>> GetOrdersByCustomerId(int customerId)
        //{
        //    // Lấy danh sách các đơn hàng liên quan đến customer_id
        //    var orders = await _context.Orders
        //        .Where(o => o.customer_id == customerId)
        //        .ToListAsync();

        //    // Kiểm tra nếu không có đơn hàng nào
        //    if (orders == null || !orders.Any())
        //    {
        //        return NotFound($"Không tìm thấy đơn hàng nào cho khách hàng với ID: {customerId}");
        //    }

        //    return Ok(orders); // Trả về danh sách đơn hàng
        //}
        // GET: api/Orders/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<Orders>> GetLatestOrderByCustomerId(int customerId)
        {
            // Lấy đơn hàng mới nhất liên quan đến customer_id
            var order = await _context.Orders
                .Where(o => o.customer_id == customerId)
                .OrderByDescending(o => o.create_at) // Sắp xếp theo ngày tạo giảm dần
                .FirstOrDefaultAsync(); // Lấy đơn hàng đầu tiên

            // Kiểm tra nếu không tìm thấy đơn hàng nào
            if (order == null)
            {
                return NotFound($"Không tìm thấy đơn hàng nào cho khách hàng với ID: {customerId}");
            }

            return Ok(order); // Trả về đối tượng đơn hàng
        }



        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.order_id == id);
        }
    }
}

