using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    [Table("Order_Details")]
    public class Order_Details
    {
        [Key]
        public int id { get; set; }
        public int order_id { get; set; }
        public int product_id { get; set; }
        public decimal price { get; set; }
        public int number_of_products { get; set; }
        public decimal total_money { get; set; }

    }
}
