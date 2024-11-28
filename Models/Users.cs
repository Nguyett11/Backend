using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int user_id { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? password { get; set; }
        public string? address { get; set; }
        public DateTime create_at { get; set; }
        public int role_id { get; set; }
    }
}
