using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.DataConnection;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Order_DetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public Order_DetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Order_Details
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order_Details>>> GetOrder_Details()
        {
            return await _context.Order_Details.ToListAsync();
        }

        // GET: api/Order_Details/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order_Details>> GetOrder_Details(int id)
        {
            var order_Details = await _context.Order_Details.FindAsync(id);

            if (order_Details == null)
            {
                return NotFound();
            }

            return order_Details;
        }

        // GET: api/Order_Details/order_id/{orderId}
        //[HttpGet("order_id/{orderId}")]
        //public async Task<ActionResult<IEnumerable<Order_Details>>> GetOrderDetailsByOrderId(int orderId)
        //{
        //    // Tìm tất cả Order_Details với order_id tương ứng
        //    var orderDetails = await _context.Order_Details
        //                                      .Where(od => od.order_id == orderId)
        //                                      .ToListAsync();

        //    if (orderDetails == null || orderDetails.Count == 0)
        //    {
        //        return NotFound();
        //    }

        //    return orderDetails;
        //}
        // GET: api/Order_Details/order_id/{orderId}
        [HttpGet("order_id/{orderId}")]
        public async Task<ActionResult<object>> GetOrderDetailsByOrderId(int orderId)
        {
            // Lấy danh sách các bản ghi chi tiết đơn hàng
            var orderDetails = await _context.Order_Details
                                              .Where(od => od.order_id == orderId)
                                              .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound("No order details found for this order ID.");
            }

            // Nếu chỉ có một bản ghi, trả về dưới dạng Object
            if (orderDetails.Count == 1)
            {
                return Ok(orderDetails.First());
            }

            // Nếu có nhiều bản ghi, trả về danh sách
            return Ok(orderDetails);
        }



        // PUT: api/Order_Details/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder_Details(int id, Order_Details order_Details)
        {
            if (id != order_Details.id)
            {
                return BadRequest();
            }

            _context.Entry(order_Details).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Order_DetailsExists(id))
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

        // POST: api/Order_Details
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order_Details>> PostOrder_Details(Order_Details order_Details)
        {
            _context.Order_Details.Add(order_Details);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder_Details", new { id = order_Details.id }, order_Details);
        }

        // DELETE: api/Order_Details/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteOrder_Details(int id)
        //{
        //    var order_Details = await _context.Order_Details.FindAsync(id);
        //    if (order_Details == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Order_Details.Remove(order_Details);
        //    await _context.SaveChangesAsync();
        //    return NoContent();
        //}

        // DELETE: api/Order_Details/{orderId}
        [HttpDelete("delete/{orderId}")]
        public async Task<IActionResult> DeleteOrder_Details(int orderId)
        {
            var orderDetails = await _context.Order_Details
                                             .FirstOrDefaultAsync(od => od.order_id == orderId);

            if (orderDetails == null)
            {
                return NotFound();
            }

            _context.Order_Details.Remove(orderDetails);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool Order_DetailsExists(int id)
        {
            return _context.Order_Details.Any(e => e.id == id);
        }
    }
}
