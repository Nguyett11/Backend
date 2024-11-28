using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    [Table("Categories")]
    public class Categories
    {
        [Key]
        public int category_id { get; set; }
        public string? category_name { get; set; }

        // Liên kết với sản phẩm
        //public ICollection<Products> products { get; set; }

    }
}
