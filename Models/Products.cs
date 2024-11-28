using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    [Table("Products")]
    public class Products
    {
        [Key]
        public int product_id { get; set; }
        public string? product_name { get; set; }
        public int brand_id { get; set; }
        public int category_id { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public string? image_url { get; set; }

        // Thêm thuộc tính liên kết đến Categories
        //[ForeignKey("category_id")]
        //public Categories? Category { get; set; }
        
    }
}
