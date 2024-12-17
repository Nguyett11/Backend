using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    [Table("Orders")]
    public class Orders
    {
        [Key]
        public int order_id { get; set; }

        [Required]
        public int customer_id { get; set; }

        [Required]
        public string order_status { get; set; }

        public DateTime? create_at { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal total_amount { get; set; }

    }
}
