using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    [Table("Brands")]
    public class Brands
    {
        [Key]
        public int brand_id { get; set; }
        public string? brand_name { get; set; }
        public int category_id { get; set; }
    }
}
