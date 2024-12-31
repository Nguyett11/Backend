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
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProducts(int id)
        {
            var products = await _context.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound();
            }

            return products;
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducts(int id, Products products)
        {
            if (id != products.product_id)
            {
                return BadRequest();
            }

            _context.Entry(products).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductsExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Products>> PostProducts(Products products)
        {
            _context.Products.Add(products);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProducts", new { id = products.product_id }, products);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducts(int id)
        {
            var products = await _context.Products.FindAsync(id);
            if (products == null)
            {
                return NotFound();
            }

            _context.Products.Remove(products);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.product_id == id);
        }

        //Lấy sản phẩm theo loại
        // GET: api/Products/ByCategory/5
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.category_id == categoryId)
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound(new { Message = "No products found for the specified category." });
            }

            return Ok(products);
        }

        //Lấy sản phẩm theo Thương hiệu
        // GET: api/Products/ByBrand/5
        [HttpGet("ByBrand/{brandId}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByBrand(int brandId)
        {
            var products = await _context.Products
                .Where(p => p.brand_id == brandId)
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound(new { Message = "No products found for the specified category." });
            }

            return Ok(products);
        }

        // Lấy danh sách sản phẩm dựa vào category_id và brand_id
        [HttpGet("categories/{category_id}/brands/{brand_id}")]
        public async Task<IActionResult> GetProductsByCategoryAndBrand(long category_id, long brand_id)
        {
            var result = await _context.Products
                .Where(p => p.category_id == category_id && p.brand_id == brand_id)
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("by-ids")]
        public async Task<IActionResult> GetProductsByIds([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return BadRequest(new { error = "Product IDs cannot be empty." });
            }

            try
            {
                // Parse danh sách IDs từ chuỗi query
                var productIds = ids.Split(',').Select(int.Parse).ToList();

                // Lấy danh sách sản phẩm từ database
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.product_id))
                    .ToListAsync();

                if (!products.Any())
                {
                    return NotFound(new { message = "No products found for the given IDs." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }


        //Post Ảnh
        [Route("SaveFile")]
        [HttpPost]
        public JsonResult SaveFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string fileName = postedFile.FileName;
                var physicalPath = _env.ContentRootPath + "/Photos/" + fileName;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                return new JsonResult(fileName);
            }
            catch (Exception)
            {
                return new JsonResult("com.jpg");
            }
        }

        //Lọc theo giá
        // GET: api/Products/ByPriceCategory/{priceCategory}
        //[HttpGet("categories/{category_id}/ByPriceCategory/{priceCategory}")]
        //public async Task<ActionResult<IEnumerable<Products>>> GetProductsByPriceCategory(string priceCategory)
        //{
        //    IQueryable<Products> productsQuery = _context.Products;

        //    switch (priceCategory.ToLower())
        //    {
        //        case "low":
        //            productsQuery = productsQuery.Where(p => p.price < 1000000);
        //            break;
        //        case "medium":
        //            productsQuery = productsQuery.Where(p => p.price >= 1000000 && p.price < 5000000);
        //            break;
        //        case "high":
        //            productsQuery = productsQuery.Where(p => p.price >= 5000000 && p.price < 10000000);
        //            break;
        //        case "premium":
        //            productsQuery = productsQuery.Where(p => p.price >= 10000000);
        //            break;
        //        default:
        //            return BadRequest(new { Message = "Invalid price category. Valid options are: low, medium, high, premium." });
        //    }

        //    var products = await productsQuery.ToListAsync();

        //    if (products == null || products.Count == 0)
        //    {
        //        return NotFound(new { Message = "No products found in the specified price category." });
        //    }

        //    return Ok(products);
        //}
        [HttpGet("categories/{category_id}/ByPriceCategory/{priceCategory}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByPriceCategory(int category_id, string priceCategory)
        {
            // Khởi tạo truy vấn
            IQueryable<Products> productsQuery = _context.Products.AsNoTracking();

            // Lọc theo danh mục
            productsQuery = productsQuery.Where(p => p.category_id == category_id);

            // Lọc theo giá
            switch (priceCategory.ToLower())
            {
                case "low":
                    productsQuery = productsQuery.Where(p => p.price < 1000000);
                    break;
                case "medium":
                    productsQuery = productsQuery.Where(p => p.price >= 1000000 && p.price < 5000000);
                    break;
                case "high":
                    productsQuery = productsQuery.Where(p => p.price >= 5000000 && p.price < 10000000);
                    break;
                case "premium":
                    productsQuery = productsQuery.Where(p => p.price >= 10000000);
                    break;
                default:
                    return UnprocessableEntity(new { Message = "Invalid price category. Valid options are: low, medium, high, premium." });
            }

            // Lấy danh sách sản phẩm
            var products = await productsQuery.ToListAsync();

            // Xử lý kết quả
            if (!products.Any())
            {
                return NotFound(new { Message = "No products found in the specified category and price range." });
            }

            return Ok(products);
        }


        //Get: tìm kiếm
        [HttpGet("search/{searchText}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetTimKiem(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest("Văn bản tìm kiếm không được để trống.");
            }

            try
            {
                var products = await _context.Products
                    .Where(p => EF.Functions.Like(p.product_name, $"%{searchText}%"))
                    .ToListAsync();

                if (products == null || !products.Any())
                {
                    return NotFound();
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                // Ghi log ngoại lệ sử dụng một framework ghi log
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }



    }
}
